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

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecFactoryDesc(
    QualifiedTypeModel ReturnType,
    string FactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    IEnumerable<QualifiedTypeModel> Parameters,
    IEnumerable<SpecFactoryRequiredPropertyDesc> RequiredProperties,
    SpecFactoryMethodFabricationMode FabricationMode,
    bool isPartial,
    Location Location
) : IDescriptor {
    private static bool TryGetConstructorFactoryFabricationMode(
        ITypeSymbol constructorFactoryType,
        Location constructorFactoryLocation,
        ExtractorContext extractorCtx,
        out SpecFactoryMethodFabricationMode fabricationMode
    ) {
        var factoryAttribute = constructorFactoryType.TryGetFactoryAttribute().GetOrThrow(extractorCtx);
        if (factoryAttribute == null) {
            // This is an implicit constructor factory.
            fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
            return false;
        }

        fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
            factoryAttribute,
            constructorFactoryLocation,
            extractorCtx);
        return true;
    }

    private static bool TryGetFactoryFabricationMode(
        ISymbol factorySymbol,
        Location factoryLocation,
        ExtractorContext extractorCtx,
        out SpecFactoryMethodFabricationMode fabricationMode
    ) {
        var factoryAttribute = factorySymbol.TryGetFactoryAttribute().GetOrThrow(extractorCtx);
        if (factoryAttribute == null) {
            // This is not a factory method.
            fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
            return false;
        }

        var factoryReferenceAttribute =
            factorySymbol.TryGetFactoryReferenceAttribute().GetOrThrow(extractorCtx);
        if (factoryReferenceAttribute != null) {
            // Cannot be a factory and a factory reference.
            throw Diagnostics.InvalidSpecification.AsException(
                "Method or Property cannot have both Factory and FactoryReference attributes.",
                factoryLocation,
                extractorCtx);
        }

        fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
            factoryAttribute,
            factoryLocation,
            extractorCtx);
        return true;
    }

    private static bool TryGetFactoryReferenceFabricationMode(
        ISymbol factoryReferenceSymbol,
        Location factoryReferenceLocation,
        ExtractorContext extractorCtx,
        out SpecFactoryMethodFabricationMode fabricationMode
    ) {
        var factoryReferenceAttribute =
            factoryReferenceSymbol.TryGetFactoryReferenceAttribute().GetOrThrow(extractorCtx);

        if (factoryReferenceAttribute == null) {
            // This is not a factory reference.
            fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
            return false;
        }

        var factoryAttribute = factoryReferenceSymbol.TryGetFactoryAttribute().GetOrThrow(extractorCtx);
        if (factoryAttribute != null) {
            // Cannot be a factory and a factory reference.
            throw Diagnostics.InvalidSpecification.AsException(
                "Property or Field cannot have both Factory and FactoryReference attributes.",
                factoryReferenceLocation,
                extractorCtx);
        }

        fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
            factoryReferenceAttribute,
            factoryReferenceLocation,
            extractorCtx);
        return true;
    }

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

        var qualifier = MetadataHelpers.TryGetQualifier(factoryReferenceSymbol)
            .GetOrThrow(extractorCtx);
        var returnTypeModel = TypeModel.FromTypeSymbol(typeArguments[typeArguments.Length - 1]);
        returnType = new QualifiedTypeModel(
            returnTypeModel,
            qualifier);

        parameterTypes = typeArguments.Length == 1
            ? ImmutableList.Create<QualifiedTypeModel>()
            : typeArguments.Take(typeArguments.Length - 1)
                .Select(typeArgument => TypeModel.FromTypeSymbol(typeArgument))
                .Select(typeModel => new QualifiedTypeModel(typeModel, QualifiedTypeModel.NoQualifier))
                .ToImmutableList();
    }

    private static IResult<bool> GetIsPartial(ISymbol factorySymbol) {
        var partialAttributes = factorySymbol.TryGetPartialAttribute();
        return partialAttributes.Map(it => Result.Ok(it != null));
    }

    public interface IExtractor {
        SpecFactoryDesc ExtractAutoConstructorFactory(
            QualifiedTypeModel factoryType,
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
        public SpecFactoryDesc ExtractAutoConstructorFactory(
            QualifiedTypeModel factoryType,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(factoryType.TypeModel.typeSymbol);
            var factorySymbol = factoryType.TypeModel.typeSymbol;
            var factoryLocation = factorySymbol.Locations.First();
            TryGetConstructorFactoryFabricationMode(
                factorySymbol,
                factoryLocation,
                currentCtx,
                out var fabricationMode);

            var constructorParameterTypes =
                MetadataHelpers.TryGetConstructorParameterQualifiedTypes(factorySymbol, currentCtx);
            var requiredProperties = MetadataHelpers
                .GetRequiredPropertyQualifiedTypes(factorySymbol, currentCtx)
                .Select(property => new SpecFactoryRequiredPropertyDesc(property.Value, property.Key, factoryLocation));
            var qualifier = MetadataHelpers.TryGetQualifier(factorySymbol)
                .GetOrThrow(currentCtx);
            var returnType = factoryType with {
                Qualifier = qualifier
            };

            return new SpecFactoryDesc(
                returnType,
                factoryType.TypeModel.GetVariableName(),
                SpecFactoryMemberType.Constructor,
                constructorParameterTypes,
                requiredProperties,
                fabricationMode,
                false, // Constructor factories cannot be partial
                factoryLocation);
        }

        public SpecFactoryDesc? ExtractFactory(
            IMethodSymbol factoryMethod,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(factoryMethod);
            var factoryLocation = factoryMethod.Locations.First();

            if (!TryGetFactoryFabricationMode(factoryMethod, factoryLocation, currentCtx, out var fabricationMode)) {
                // This is not a factory.
                return null;
            }

            var methodParameterTypes =
                MetadataHelpers.TryGetMethodParametersQualifiedTypes(factoryMethod, currentCtx);

            var qualifier = MetadataHelpers.TryGetQualifier(factoryMethod)
                .GetOrThrow(currentCtx);
            var returnTypeModel = TypeModel.FromTypeSymbol(factoryMethod.ReturnType);
            var returnType = new QualifiedTypeModel(
                returnTypeModel,
                qualifier);

            var isPartial = GetIsPartial(factoryMethod).GetOrThrow(currentCtx);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryLocation, currentCtx);

            return new SpecFactoryDesc(
                returnType,
                factoryMethod.Name,
                SpecFactoryMemberType.Method,
                methodParameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryLocation);
        }

        public SpecFactoryDesc? ExtractFactory(
            IPropertySymbol factoryProperty,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(factoryProperty);
            var factoryLocation = factoryProperty.Locations.First();

            if (!TryGetFactoryFabricationMode(
                factoryProperty,
                factoryLocation,
                currentCtx,
                out var fabricationMode)
            ) {
                // This is not a factory.
                return null;
            }

            var methodParameterTypes = ImmutableList.Create<QualifiedTypeModel>();

            var qualifier = MetadataHelpers.TryGetQualifier(factoryProperty)
                .GetOrThrow(currentCtx);
            var returnTypeModel = TypeModel.FromTypeSymbol(factoryProperty.Type);
            var returnType = new QualifiedTypeModel(
                returnTypeModel,
                qualifier);

            var isPartial = GetIsPartial(factoryProperty).GetOrThrow(currentCtx);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryLocation, currentCtx);

            return new SpecFactoryDesc(
                returnType,
                factoryProperty.Name,
                SpecFactoryMemberType.Property,
                methodParameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryLocation);
        }

        public SpecFactoryDesc? ExtractFactoryReference(
            IPropertySymbol factoryReferenceProperty,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(factoryReferenceProperty);
            var factoryReferenceLocation = factoryReferenceProperty.Locations.First();
            if (!TryGetFactoryReferenceFabricationMode(factoryReferenceProperty,
                factoryReferenceLocation,
                currentCtx,
                out var fabricationMode)
            ) {
                // This is not a factory reference.
                return null;
            }

            GetFactoryReferenceTypes(
                factoryReferenceProperty,
                factoryReferenceProperty.Type,
                factoryReferenceLocation,
                currentCtx,
                out var returnType,
                out var parameterTypes);
            var isPartial = GetIsPartial(factoryReferenceProperty).GetOrThrow(currentCtx);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryReferenceLocation, currentCtx);

            return new SpecFactoryDesc(
                returnType,
                factoryReferenceProperty.Name,
                SpecFactoryMemberType.Reference,
                parameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryReferenceLocation);
        }

        public SpecFactoryDesc? ExtractFactoryReference(
            IFieldSymbol factoryReferenceField,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(factoryReferenceField);
            var factoryReferenceLocation = factoryReferenceField.Locations.First();
            if (!TryGetFactoryReferenceFabricationMode(factoryReferenceField,
                factoryReferenceLocation,
                currentCtx,
                out var fabricationMode)
            ) {
                // This is not a factory reference.
                return null;
            }

            GetFactoryReferenceTypes(
                factoryReferenceField,
                factoryReferenceField.Type,
                factoryReferenceLocation,
                currentCtx,
                out var returnType,
                out var parameterTypes);
            var isPartial = GetIsPartial(factoryReferenceField).GetOrThrow(currentCtx);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryReferenceLocation, currentCtx);

            return new SpecFactoryDesc(
                returnType,
                factoryReferenceField.Name,
                SpecFactoryMemberType.Reference,
                parameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryReferenceLocation);
        }
    }
}
