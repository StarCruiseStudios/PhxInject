// -----------------------------------------------------------------------------
//  <copyright file="SpecFactoryMetadata.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record SpecFactoryMetadata(
    QualifiedTypeModel ReturnType,
    string FactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    IEnumerable<QualifiedTypeModel> Parameters,
    IEnumerable<AutoFactoryRequiredPropertyMetadata> RequiredProperties,
    FactoryFabricationMode FabricationMode,
    bool isPartial,
    PartialAttributeMetadata? PartialAttributeMetadata,
    FactoryAttributeMetadata? FactoryAttributeMetadata,
    FactoryReferenceAttributeMetadata? FactoryReferenceAttributeMetadata,
    ISymbol FactorySymbol
) : IMetadata {
    public Location Location {
        get => FactorySymbol.GetLocationOrDefault();
    }

    public interface IFactoryExtractor {
        bool CanExtract(ISymbol factorySymbol);
        SpecFactoryMetadata ExtractFactory(
            ISymbol factorySymbol,
            ExtractorContext parentCtx
        );
    }

    public class FactoryExtractor(
        QualifierMetadata.IAttributeExtractor qualifierExtractor,
        PartialAttributeMetadata.IExtractor partialAttributeExtractor,
        FactoryAttributeMetadata.IExtractor factoryAttributeExtractor,
        FactoryReferenceAttributeMetadata.IExtractor factoryReferenceAttributeExtractor
    ) : IFactoryExtractor {
        public static readonly IFactoryExtractor Instance =
            new FactoryExtractor(
                QualifierMetadata.AttributeExtractor.Instance,
                PartialAttributeMetadata.Extractor.Instance,
                FactoryAttributeMetadata.Extractor.Instance,
                FactoryReferenceAttributeMetadata.Extractor.Instance
            );

        public bool CanExtract(ISymbol factorySymbol) {
            return VerifyExtract(factorySymbol, null);
        }

        public SpecFactoryMetadata ExtractFactory(
            ISymbol factorySymbol,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting specification factory {factorySymbol}",
                factorySymbol,
                currentCtx => {
                    VerifyExtract(factorySymbol, currentCtx);

                    var factoryAttribute = factoryAttributeExtractor.ExtractFactory(factorySymbol, currentCtx);
                    var fabricationMode = factoryAttribute.FabricationMode;
                    var (specFactoryMemberType,
                        parameters,
                        returnTypeSymbol
                        ) = factorySymbol switch {
                        IMethodSymbol methodSymbol => (
                            SpecFactoryMemberType.Method,
                            methodSymbol.Parameters,
                            methodSymbol.ReturnType
                        ),
                        IPropertySymbol propertySymbol => (
                            SpecFactoryMemberType.Property,
                            propertySymbol.Parameters,
                            propertySymbol.Type
                        ),
                        _ => throw Diagnostics.InternalError.AsException(
                            $"Factory {factorySymbol} must be a method or property.",
                            factorySymbol.GetLocationOrDefault(),
                            currentCtx
                        )
                    };

                    IReadOnlyList<QualifiedTypeModel> methodParameterTypes = parameters
                        .Select(parameter => {
                            var qualifier = qualifierExtractor.Extract(parameter, currentCtx);
                            return parameter.Type.ToQualifiedTypeModel(qualifier);
                        })
                        .ToImmutableList();

                    var requiredProperties = ImmutableList<AutoFactoryRequiredPropertyMetadata>.Empty;

                    var qualifier = qualifierExtractor.Extract(factorySymbol, currentCtx);
                    var returnType = returnTypeSymbol.ToQualifiedTypeModel(qualifier);

                    var partialAttribute = partialAttributeExtractor.CanExtract(factorySymbol)
                        ? partialAttributeExtractor.Extract(returnType.TypeModel, factorySymbol, currentCtx)
                        : null;
                    var isPartial = partialAttribute != null;
                    var factoryMemberName = factorySymbol.Name;

                    return new SpecFactoryMetadata(
                        returnType,
                        factoryMemberName,
                        specFactoryMemberType,
                        methodParameterTypes,
                        requiredProperties,
                        fabricationMode,
                        isPartial,
                        partialAttribute,
                        factoryAttribute,
                        null,
                        factorySymbol);
                });
        }

        private bool VerifyExtract(ISymbol factorySymbol, ExtractorContext? currentCtx) {
            if (!factoryAttributeExtractor.CanExtract(factorySymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Factory {factorySymbol} must have a {FactoryAttributeMetadata.FactoryAttributeClassName}.",
                        factorySymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (factoryReferenceAttributeExtractor.CanExtract(factorySymbol)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Factory {factorySymbol} cannot have both {FactoryAttributeMetadata.FactoryAttributeClassName} and {FactoryReferenceAttributeMetadata.FactoryReferenceAttributeClassName}.",
                        factorySymbol.GetLocationOrDefault(),
                        currentCtx);
                }

                if (factorySymbol is not { DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                    or not IMethodSymbol and not IPropertySymbol
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Factory {factorySymbol} must be a public or internal method or property.",
                        factorySymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }

    public interface IFactoryReferenceExtractor {
        bool CanExtract(ISymbol factoryReferenceSymbol);
        SpecFactoryMetadata ExtractFactoryReference(
            ISymbol factoryReferenceSymbol,
            ExtractorContext parentCtx
        );
    }

    public class FactoryReferenceExtractor(
        QualifierMetadata.IAttributeExtractor qualifierExtractor,
        PartialAttributeMetadata.IExtractor partialAttributeExtractor,
        FactoryAttributeMetadata.IExtractor factoryAttributeExtractor,
        FactoryReferenceAttributeMetadata.IExtractor factoryReferenceAttributeExtractor
    ) : IFactoryReferenceExtractor {
        public static readonly IFactoryReferenceExtractor Instance =
            new FactoryReferenceExtractor(
                QualifierMetadata.AttributeExtractor.Instance,
                PartialAttributeMetadata.Extractor.Instance,
                FactoryAttributeMetadata.Extractor.Instance,
                FactoryReferenceAttributeMetadata.Extractor.Instance
            );

        public bool CanExtract(ISymbol factoryReferenceSymbol) {
            return VerifyExtract(factoryReferenceSymbol, null);
        }

        public SpecFactoryMetadata ExtractFactoryReference(
            ISymbol factoryReferenceSymbol,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting specification factory reference {factoryReferenceSymbol}",
                factoryReferenceSymbol,
                currentCtx => {
                    VerifyExtract(factoryReferenceSymbol, currentCtx);

                    var factoryReferenceAttribute =
                        factoryReferenceAttributeExtractor.Extract(factoryReferenceSymbol, currentCtx);
                    var fabricationMode = factoryReferenceAttribute.FabricationMode;
                    var factoryReferenceTypeSymbol = factoryReferenceSymbol switch {
                        IFieldSymbol fieldSymbol => fieldSymbol.Type,
                        IPropertySymbol propertySymbol => propertySymbol.Type,
                        _ => throw Diagnostics.InternalError.AsException(
                            $"Factory reference {factoryReferenceSymbol} must be a field or property.",
                            factoryReferenceSymbol.GetLocationOrDefault(),
                            currentCtx
                        )
                    };

                    if (factoryReferenceTypeSymbol is not INamedTypeSymbol factoryReferenceNamedTypeSymbol
                        || factoryReferenceNamedTypeSymbol.GetFullyQualifiedBaseName() != TypeNames.FuncClassName
                    ) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Factory reference of type {factoryReferenceTypeSymbol.GetFullyQualifiedBaseName()} must be of type {TypeNames.FuncClassName}.",
                            factoryReferenceSymbol.GetLocationOrDefault(),
                            currentCtx);
                    }

                    var typeArguments = factoryReferenceNamedTypeSymbol.TypeArguments;
                    if (typeArguments.Length == 0) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            "Factory reference func must have a return type.",
                            factoryReferenceSymbol.GetLocationOrDefault(),
                            currentCtx);
                    }

                    IReadOnlyList<QualifiedTypeModel> parameterTypes = typeArguments.Take(typeArguments.Length - 1)
                        .Select(parameter => {
                            var qualifier = qualifierExtractor.Extract(parameter, currentCtx);
                            return parameter.ToQualifiedTypeModel(qualifier);
                        })
                        .ToImmutableList();

                    var requiredProperties = ImmutableList<AutoFactoryRequiredPropertyMetadata>.Empty;

                    var qualifier = qualifierExtractor.Extract(factoryReferenceSymbol, currentCtx);
                    var returnType = typeArguments.Last().ToQualifiedTypeModel(qualifier);

                    var partialAttribute = partialAttributeExtractor.CanExtract(factoryReferenceSymbol)
                        ? partialAttributeExtractor.Extract(returnType.TypeModel, factoryReferenceSymbol, currentCtx)
                        : null;
                    var isPartial = partialAttribute != null;
                    var factoryMemberName = factoryReferenceSymbol.Name;

                    return new SpecFactoryMetadata(
                        returnType,
                        factoryMemberName,
                        SpecFactoryMemberType.Reference,
                        parameterTypes,
                        requiredProperties,
                        fabricationMode,
                        isPartial,
                        partialAttribute,
                        null,
                        factoryReferenceAttribute,
                        factoryReferenceSymbol);
                });
        }

        private bool VerifyExtract(ISymbol factoryReferenceSymbol, ExtractorContext? currentCtx) {
            if (!factoryReferenceAttributeExtractor.CanExtract(factoryReferenceSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Factory reference {factoryReferenceSymbol} must have a {FactoryReferenceAttributeMetadata.FactoryReferenceAttributeClassName}.",
                        factoryReferenceSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (factoryAttributeExtractor.CanExtract(factoryReferenceSymbol)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Factory reference {factoryReferenceSymbol} cannot have both {FactoryAttributeMetadata.FactoryAttributeClassName} and {FactoryReferenceAttributeMetadata.FactoryReferenceAttributeClassName}.",
                        factoryReferenceSymbol.GetLocationOrDefault(),
                        currentCtx);
                }

                if (factoryReferenceSymbol is not
                    { DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                    or not IFieldSymbol and not IPropertySymbol
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Factory reference {factoryReferenceSymbol} must be a public or internal field or property.",
                        factoryReferenceSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }

    public interface IAutoFactoryExtractor {
        SpecFactoryMetadata ExtractFactory(QualifiedTypeModel constructorType, ExtractorContext parentCtx);
    }

    public class AutoFactoryExtractor(
        AutoFactoryConstructorMetadata.IExtractor autoFactoryConstructorExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor,
        FactoryAttributeMetadata.IExtractor factoryAttributeExtractor
    ) : IAutoFactoryExtractor {
        public static readonly IAutoFactoryExtractor Instance =
            new AutoFactoryExtractor(
                AutoFactoryConstructorMetadata.Extractor.Instance,
                QualifierMetadata.AttributeExtractor.Instance,
                FactoryAttributeMetadata.Extractor.Instance
            );

        public SpecFactoryMetadata ExtractFactory(
            QualifiedTypeModel constructorType,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting auto factory {constructorType}",
                constructorType.TypeModel.TypeSymbol,
                currentCtx => {
                    var constructorTypeSymbol = constructorType.TypeModel.TypeSymbol;
                    var factoryAttribute = factoryAttributeExtractor.CanExtract(constructorTypeSymbol)
                        ? factoryAttributeExtractor.ExtractAutoFactory(constructorTypeSymbol, currentCtx)
                        : null;
                    var fabricationMode = factoryAttribute?.FabricationMode ?? FactoryFabricationMode.Recurrent;

                    var constructor = autoFactoryConstructorExtractor.Extract(constructorTypeSymbol, currentCtx);
                    var constructorParameterTypes = constructor.ParameterTypes;
                    var requiredProperties = constructor.RequiredProperties;

                    var qualifier = qualifierExtractor.Extract(constructorTypeSymbol, currentCtx);
                    var returnType = constructorType with {
                        Qualifier = qualifier
                    };

                    var isPartial = false;
                    var factoryMemberName = constructorType.TypeModel.GetVariableName();

                    return new SpecFactoryMetadata(
                        returnType,
                        factoryMemberName,
                        SpecFactoryMemberType.Constructor,
                        constructorParameterTypes,
                        requiredProperties,
                        fabricationMode,
                        isPartial,
                        null,
                        factoryAttribute,
                        null,
                        constructorTypeSymbol);
                });
        }
    }
}
