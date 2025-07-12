// -----------------------------------------------------------------------------
//  <copyright file="SpecFactoryDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecFactoryDesc(
    QualifiedTypeModel ReturnType,
    string FactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    IEnumerable<QualifiedTypeModel> Parameters,
    IEnumerable<SpecFactoryRequiredPropertyDesc> RequiredProperties,
    FactoryFabricationMode FabricationMode,
    bool isPartial,
    Location Location
) : IDescriptor {
    private static void GetFactoryReferenceTypes(
        ISymbol factoryReferenceSymbol,
        ITypeSymbol factoryReferenceTypeSymbol,
        Location factoryReferenceLocation,
        ExtractorContext extractorCtx,
        out QualifiedTypeModel returnType,
        out IEnumerable<QualifiedTypeModel> parameterTypes
    ) {
        var referenceTypeSymbol = factoryReferenceTypeSymbol as INamedTypeSymbol;
        if (referenceTypeSymbol == null || referenceTypeSymbol.Name != "Func") {
            // Not the correct type to be a factory reference.
            throw Diagnostics.InvalidSpecification.AsException(
                "Factory reference must be a field or property of type Func<>.",
                factoryReferenceLocation,
                extractorCtx);
        }

        var typeArguments = referenceTypeSymbol.TypeArguments;

        var qualifier = QualifierMetadata.Extractor.Instance.Extract(factoryReferenceSymbol, extractorCtx);
        var returnTypeModel = TypeModel.FromTypeSymbol(typeArguments[typeArguments.Length - 1]);
        returnType = new QualifiedTypeModel(
            returnTypeModel,
            qualifier);

        parameterTypes = typeArguments.Length == 1
            ? ImmutableList.Create<QualifiedTypeModel>()
            : typeArguments.Take(typeArguments.Length - 1)
                .Select(typeArgument => TypeModel.FromTypeSymbol(typeArgument))
                .Select(typeModel => new QualifiedTypeModel(typeModel, QualifierMetadata.NoQualifier))
                .ToImmutableList();
    }

    public interface IExtractor {
        SpecFactoryDesc ExtractAutoConstructorFactory(
            QualifiedTypeModel constructorType,
            ExtractorContext extractorCtx
        );
        SpecFactoryDesc? ExtractFactory(
            IMethodSymbol factoryMethod,
            ExtractorContext extractorCtx
        );
        SpecFactoryDesc? ExtractFactory(
            IPropertySymbol factoryProperty,
            ExtractorContext extractorCtx
        );
        SpecFactoryDesc? ExtractFactoryReference(
            IPropertySymbol factoryReferenceProperty,
            ExtractorContext extractorCtx
        );
        SpecFactoryDesc? ExtractFactoryReference(
            IFieldSymbol factoryReferenceField,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly FactoryAttributeMetadata.IExtractor factoryAttributeExtractor;
        private readonly FactoryReferenceAttributeMetadata.IExtractor factoryReferenceAttributeExtractor;
        private readonly PartialAttributeMetadata.IExtractor partialAttributeExtractor;
        public Extractor(
            PartialAttributeMetadata.IExtractor partialAttributeExtractor,
            FactoryAttributeMetadata.IExtractor factoryAttributeExtractor,
            FactoryReferenceAttributeMetadata.IExtractor factoryReferenceAttributeExtractor
        ) {
            this.partialAttributeExtractor = partialAttributeExtractor;
            this.factoryAttributeExtractor = factoryAttributeExtractor;
            this.factoryReferenceAttributeExtractor = factoryReferenceAttributeExtractor;
        }

        public Extractor() : this(
            PartialAttributeMetadata.Extractor.Instance,
            FactoryAttributeMetadata.Extractor.Instance,
            FactoryReferenceAttributeMetadata.Extractor.Instance
        ) { }

        public SpecFactoryDesc ExtractAutoConstructorFactory(
            QualifiedTypeModel constructorType,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                "extracting auto constructor factory specification",
                constructorType.TypeModel.TypeSymbol,
                currentCtx => {
                    var constructorSymbol = constructorType.TypeModel.TypeSymbol;
                    var constructorLocation = constructorSymbol.Locations.First();
                    var factoryAttribute = factoryAttributeExtractor.CanExtract(constructorSymbol)
                        ? factoryAttributeExtractor.ExtractAutoFactory(constructorSymbol, currentCtx)
                        : null;

                    var constructorParameterTypes =
                        MetadataHelpers.TryGetConstructorParameterQualifiedTypes(constructorSymbol, currentCtx);
                    var requiredProperties = MetadataHelpers
                        .GetRequiredPropertyQualifiedTypes(constructorSymbol, currentCtx)
                        .Select(property =>
                            new SpecFactoryRequiredPropertyDesc(property.Value, property.Key, constructorLocation));
                    var qualifier = QualifierMetadata.Extractor.Instance.Extract(constructorSymbol, currentCtx);
                    var returnType = constructorType with {
                        Qualifier = qualifier
                    };

                    return new SpecFactoryDesc(
                        returnType,
                        constructorType.TypeModel.GetVariableName(),
                        SpecFactoryMemberType.Constructor,
                        constructorParameterTypes,
                        requiredProperties,
                        factoryAttribute?.FabricationMode ?? FactoryFabricationMode.Recurrent,
                        false, // Constructor factories cannot be partial
                        constructorLocation);
                });
        }

        public SpecFactoryDesc? ExtractFactory(
            IMethodSymbol factoryMethod,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                "extracting specification factory method",
                factoryMethod,
                currentCtx => {
                    var factoryLocation = factoryMethod.Locations.First();

                    if (!factoryAttributeExtractor.CanExtract(factoryMethod)) {
                        // This is not a factory.
                        return null;
                    }

                    var factoryAttribute = factoryAttributeExtractor.ExtractFactory(factoryMethod, currentCtx);
                    if (factoryReferenceAttributeExtractor.CanExtract(factoryMethod)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            "Method cannot have both Factory and FactoryReference attributes.",
                            factoryLocation,
                            extractorCtx);
                    }

                    var methodParameterTypes =
                        MetadataHelpers.TryGetMethodParametersQualifiedTypes(factoryMethod, currentCtx);

                    var qualifier = QualifierMetadata.Extractor.Instance.Extract(factoryMethod, currentCtx);
                    var returnTypeModel = TypeModel.FromTypeSymbol(factoryMethod.ReturnType);
                    var returnType = new QualifiedTypeModel(
                        returnTypeModel,
                        qualifier);

                    var partialAttribute = partialAttributeExtractor.CanExtract(factoryMethod)
                        ? partialAttributeExtractor.Extract(returnType.TypeModel, factoryMethod, currentCtx)
                        : null;

                    return new SpecFactoryDesc(
                        returnType,
                        factoryMethod.Name,
                        SpecFactoryMemberType.Method,
                        methodParameterTypes,
                        ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                        factoryAttribute.FabricationMode,
                        partialAttribute != null,
                        factoryLocation);
                });
        }

        public SpecFactoryDesc? ExtractFactory(
            IPropertySymbol factoryProperty,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                "extracting specification factory property",
                factoryProperty,
                currentCtx => {
                    var factoryLocation = factoryProperty.Locations.First();

                    if (!factoryAttributeExtractor.CanExtract(factoryProperty)) {
                        // This is not a factory.
                        return null;
                    }

                    var factoryAttribute = factoryAttributeExtractor.ExtractFactory(factoryProperty, currentCtx);
                    if (factoryReferenceAttributeExtractor.CanExtract(factoryProperty)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            "Property cannot have both Factory and FactoryReference attributes.",
                            factoryLocation,
                            extractorCtx);
                    }

                    var methodParameterTypes = ImmutableList.Create<QualifiedTypeModel>();

                    var qualifier = QualifierMetadata.Extractor.Instance.Extract(factoryProperty, currentCtx);
                    var returnTypeModel = TypeModel.FromTypeSymbol(factoryProperty.Type);
                    var returnType = new QualifiedTypeModel(
                        returnTypeModel,
                        qualifier);

                    var partialAttribute = partialAttributeExtractor.CanExtract(factoryProperty)
                        ? partialAttributeExtractor.Extract(returnType.TypeModel, factoryProperty, currentCtx)
                        : null;

                    return new SpecFactoryDesc(
                        returnType,
                        factoryProperty.Name,
                        SpecFactoryMemberType.Property,
                        methodParameterTypes,
                        ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                        factoryAttribute.FabricationMode,
                        partialAttribute != null,
                        factoryLocation);
                });
        }

        public SpecFactoryDesc? ExtractFactoryReference(
            IPropertySymbol factoryReferenceProperty,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                "extracting specification factory reference property",
                factoryReferenceProperty,
                currentCtx => {
                    var factoryReferenceLocation = factoryReferenceProperty.Locations.First();

                    if (!factoryReferenceAttributeExtractor.CanExtract(factoryReferenceProperty)) {
                        // This is not a factory reference.
                        return null;
                    }

                    var factoryReferenceAttribute =
                        factoryReferenceAttributeExtractor.Extract(factoryReferenceProperty, currentCtx);
                    if (factoryAttributeExtractor.CanExtract(factoryReferenceProperty)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            "Property cannot have both Factory and FactoryReference attributes.",
                            factoryReferenceLocation,
                            extractorCtx);
                    }

                    GetFactoryReferenceTypes(
                        factoryReferenceProperty,
                        factoryReferenceProperty.Type,
                        factoryReferenceLocation,
                        currentCtx,
                        out var returnType,
                        out var parameterTypes);

                    var partialAttribute = partialAttributeExtractor.CanExtract(factoryReferenceProperty)
                        ? partialAttributeExtractor.Extract(returnType.TypeModel, factoryReferenceProperty, currentCtx)
                        : null;

                    return new SpecFactoryDesc(
                        returnType,
                        factoryReferenceProperty.Name,
                        SpecFactoryMemberType.Reference,
                        parameterTypes,
                        ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                        factoryReferenceAttribute.FabricationMode,
                        partialAttribute != null,
                        factoryReferenceLocation);
                });
        }

        public SpecFactoryDesc? ExtractFactoryReference(
            IFieldSymbol factoryReferenceField,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                "extracting specification factory reference field",
                factoryReferenceField,
                currentCtx => {
                    var factoryReferenceLocation = factoryReferenceField.Locations.First();
                    if (!factoryReferenceAttributeExtractor.CanExtract(factoryReferenceField)) {
                        // This is not a factory reference.
                        return null;
                    }

                    var factoryReferenceAttribute =
                        factoryReferenceAttributeExtractor.Extract(factoryReferenceField, currentCtx);
                    if (factoryAttributeExtractor.CanExtract(factoryReferenceField)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            "Field cannot have both Factory and FactoryReference attributes.",
                            factoryReferenceLocation,
                            extractorCtx);
                    }

                    GetFactoryReferenceTypes(
                        factoryReferenceField,
                        factoryReferenceField.Type,
                        factoryReferenceLocation,
                        currentCtx,
                        out var returnType,
                        out var parameterTypes);

                    var partialAttribute = partialAttributeExtractor.CanExtract(factoryReferenceField)
                        ? partialAttributeExtractor.Extract(returnType.TypeModel, factoryReferenceField, currentCtx)
                        : null;

                    return new SpecFactoryDesc(
                        returnType,
                        factoryReferenceField.Name,
                        SpecFactoryMemberType.Reference,
                        parameterTypes,
                        ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                        factoryReferenceAttribute.FabricationMode,
                        partialAttribute != null,
                        factoryReferenceLocation);
                });
        }
    }
}
