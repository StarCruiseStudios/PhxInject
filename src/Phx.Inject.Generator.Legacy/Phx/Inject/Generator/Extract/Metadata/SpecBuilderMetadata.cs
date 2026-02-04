// -----------------------------------------------------------------------------
// <copyright file="SpecBuilderMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record SpecBuilderMetadata(
    QualifiedTypeModel BuiltType,
    string BuilderMemberName,
    SpecBuilderMemberType SpecBuilderMemberType,
    IEnumerable<QualifiedTypeModel> Parameters,
    BuilderAttributeMetadata? BuilderAttributeMetadata,
    BuilderReferenceAttributeMetadata? BuilderReferenceAttributeMetadata,
    ISymbol BuilderSymbol
) : IMetadata {
    public Location Location {
        get => BuilderSymbol.GetLocationOrDefault();
    }

    public interface IBuilderExtractor {
        bool CanExtract(ISymbol builderSymbol);
        SpecBuilderMetadata ExtractBuilder(IMethodSymbol builderMethod, ExtractorContext parentCtx);
    }

    public class BuilderExtractor(
        BuilderAttributeMetadata.IExtractor builderAttributeExtractor,
        BuilderReferenceAttributeMetadata.IExtractor builderReferenceAttributeExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor
    ) : IBuilderExtractor {
        public static readonly IBuilderExtractor Instance =
            new BuilderExtractor(
                BuilderAttributeMetadata.Extractor.Instance,
                BuilderReferenceAttributeMetadata.Extractor.Instance,
                QualifierMetadata.AttributeExtractor.Instance
            );

        public bool CanExtract(ISymbol builderSymbol) {
            return VerifyExtract(builderSymbol, null);
        }

        public SpecBuilderMetadata ExtractBuilder(IMethodSymbol builderMethod, ExtractorContext parentCtx) {
            return parentCtx.UseChildExtractorContext(
                $"extracting specification builder {builderMethod}",
                builderMethod,
                currentCtx => {
                    VerifyExtract(builderMethod, currentCtx);

                    var builderAttribute = builderAttributeExtractor.Extract(builderMethod, currentCtx);
                    IReadOnlyList<QualifiedTypeModel> methodParameterTypes = builderMethod.Parameters
                        .Select(parameter => {
                            var qualifier = qualifierExtractor.Extract(parameter, currentCtx);
                            return parameter.Type.ToQualifiedTypeModel(qualifier);
                        })
                        .ToImmutableList();

                    if (methodParameterTypes.Count == 0) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            "Builder must have at least one parameter.",
                            builderMethod.GetLocationOrDefault(),
                            currentCtx);
                    }

                    // Get the qualifier from the builder method symbol, not the type.
                    var qualifier = qualifierExtractor.Extract(builderMethod, currentCtx);
                    var builtType = methodParameterTypes[0] with {
                        Qualifier = qualifier
                    };

                    var parameterTypes = methodParameterTypes
                        .Skip(1)
                        .ToImmutableList();
                    var builderMemberName = builderMethod.Name;

                    return new SpecBuilderMetadata(
                        builtType,
                        builderMemberName,
                        SpecBuilderMemberType.Method,
                        parameterTypes,
                        builderAttribute,
                        null,
                        builderMethod);
                });
        }

        private bool VerifyExtract(ISymbol builderSymbol, ExtractorContext? currentCtx) {
            if (!builderAttributeExtractor.CanExtract(builderSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Builder {builderSymbol} must have a {BuilderAttributeMetadata.BuilderAttributeClassName}.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (builderReferenceAttributeExtractor.CanExtract(builderSymbol)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Builder {builderSymbol} cannot have both {BuilderAttributeMetadata.BuilderAttributeClassName} and {BuilderReferenceAttributeMetadata.BuilderReferenceAttributeClassName}.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
                }

                if (builderSymbol is
                    not IMethodSymbol { DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Builder {builderSymbol} must be a public or internal method.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }

    public interface IBuilderReferenceExtractor {
        bool CanExtract(ISymbol builderSymbol);
        SpecBuilderMetadata ExtractBuilderReference(ISymbol builderReferenceSymbol, ExtractorContext parentCtx);
    }

    public class BuilderReferenceExtractor(
        BuilderAttributeMetadata.IExtractor builderAttributeExtractor,
        BuilderReferenceAttributeMetadata.IExtractor builderReferenceAttributeExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor
    ) : IBuilderReferenceExtractor {
        public static readonly IBuilderReferenceExtractor Instance =
            new BuilderReferenceExtractor(
                BuilderAttributeMetadata.Extractor.Instance,
                BuilderReferenceAttributeMetadata.Extractor.Instance,
                QualifierMetadata.AttributeExtractor.Instance
            );

        public bool CanExtract(ISymbol builderSymbol) {
            return VerifyExtract(builderSymbol, null);
        }

        public SpecBuilderMetadata ExtractBuilderReference(
            ISymbol builderReferenceSymbol,
            ExtractorContext parentCtx) {
            return parentCtx.UseChildExtractorContext(
                $"extracting specification builder reference {builderReferenceSymbol}",
                builderReferenceSymbol,
                currentCtx => {
                    VerifyExtract(builderReferenceSymbol, currentCtx);

                    var builderReferenceAttribute =
                        builderReferenceAttributeExtractor.Extract(builderReferenceSymbol, currentCtx);
                    var builderReferenceTypeSymbol = builderReferenceSymbol switch {
                        IPropertySymbol propertySymbol => propertySymbol.Type,
                        IFieldSymbol fieldSymbol => fieldSymbol.Type,
                        _ => throw Diagnostics.InternalError.AsException(
                            "Builder reference must be a property or field.",
                            builderReferenceSymbol.GetLocationOrDefault(),
                            currentCtx)
                    };

                    if (builderReferenceTypeSymbol is not INamedTypeSymbol builderReferenceNamedTypeSymbol
                        || builderReferenceTypeSymbol.GetFullyQualifiedBaseName() != TypeNames.ActionClassName
                    ) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Builder reference of type {builderReferenceTypeSymbol.GetFullyQualifiedBaseName()} must be of type {TypeNames.ActionClassName}.",
                            builderReferenceSymbol.GetLocationOrDefault(),
                            currentCtx);
                    }

                    IReadOnlyList<ITypeSymbol> typeArguments = builderReferenceNamedTypeSymbol.TypeArguments;
                    if (typeArguments.Count == 0) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            "Builder reference action must have at least one parameter.",
                            builderReferenceSymbol.GetLocationOrDefault(),
                            currentCtx);
                    }

                    // Get the qualifier from the builder reference symbol, not the type.
                    var qualifier = qualifierExtractor.Extract(builderReferenceSymbol, currentCtx);
                    var builtType = typeArguments[0].ToQualifiedTypeModel(qualifier);

                    var parameterTypes = typeArguments.Skip(1)
                        .Select(typeArgument => typeArgument.ToQualifiedTypeModel(QualifierMetadata.NoQualifier))
                        .ToImmutableList();
                    var builderMemberName = builderReferenceSymbol.Name;

                    return new SpecBuilderMetadata(
                        builtType,
                        builderMemberName,
                        SpecBuilderMemberType.Reference,
                        parameterTypes,
                        null,
                        builderReferenceAttribute,
                        builderReferenceSymbol);
                });
        }

        private bool VerifyExtract(ISymbol builderSymbol, ExtractorContext? currentCtx) {
            if (!builderReferenceAttributeExtractor.CanExtract(builderSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Builder reference {builderSymbol} must have a {BuilderReferenceAttributeMetadata.BuilderReferenceAttributeClassName}.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (builderAttributeExtractor.CanExtract(builderSymbol)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Builder reference {builderSymbol} cannot have both {BuilderAttributeMetadata.BuilderAttributeClassName} and {BuilderReferenceAttributeMetadata.BuilderReferenceAttributeClassName}.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
                }

                if (builderSymbol is not { DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                    or not IPropertySymbol and not IFieldSymbol
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Builder reference {builderSymbol} must be a public or internal property or field.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }

    public interface IAutoBuilderExtractor {
        SpecBuilderMetadata ExtractBuilder(QualifiedTypeModel autoBuilderType, ExtractorContext parentCtx);
    }

    public class AutoBuilderExtractor(
        BuilderAttributeMetadata.IExtractor builderAttributeExtractor,
        BuilderReferenceAttributeMetadata.IExtractor builderReferenceAttributeExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor
    ) : IAutoBuilderExtractor {
        public static readonly IAutoBuilderExtractor Instance =
            new AutoBuilderExtractor(
                BuilderAttributeMetadata.Extractor.Instance,
                BuilderReferenceAttributeMetadata.Extractor.Instance,
                QualifierMetadata.AttributeExtractor.Instance
            );

        public SpecBuilderMetadata ExtractBuilder(QualifiedTypeModel autoBuilderType, ExtractorContext parentCtx) {
            return parentCtx.UseChildExtractorContext(
                $"extracting auto builder for type {autoBuilderType}",
                autoBuilderType.TypeModel.TypeSymbol,
                currentCtx => {
                    IReadOnlyList<IMethodSymbol> builderMethods = autoBuilderType.TypeModel.TypeSymbol.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(methodSymbol => {
                            var qualifier = qualifierExtractor.Extract(methodSymbol, currentCtx).Qualifier;
                            return builderAttributeExtractor.CanExtract(methodSymbol)
                                && Equals(qualifier, autoBuilderType.Qualifier.Qualifier);
                        })
                        .ToImmutableList();

                    switch (builderMethods.Count) {
                        case 0:
                            throw Diagnostics.InvalidSpecification.AsException(
                                $"No auto builder method found for required auto builder type: {autoBuilderType}",
                                autoBuilderType.TypeModel.Location,
                                currentCtx);
                        case > 1:
                            throw Diagnostics.InvalidSpecification.AsException(
                                $"More than one auto builder method found for type: {autoBuilderType}",
                                autoBuilderType.TypeModel.Location,
                                currentCtx);
                    }

                    var builderMethod = builderMethods.Single();
                    VerifyExtractBuilder(autoBuilderType, builderMethod, currentCtx);
                    var builderAttribute = builderAttributeExtractor.Extract(builderMethod, currentCtx);
                    IReadOnlyList<QualifiedTypeModel> methodParameterTypes = builderMethod.Parameters
                        .Select(parameter => {
                            var qualifier = qualifierExtractor.Extract(parameter, currentCtx);
                            return parameter.Type.ToQualifiedTypeModel(qualifier);
                        })
                        .ToImmutableList();

                    // Get the qualifier from the builder method symbol, not the type.
                    var qualifier = qualifierExtractor.Extract(builderMethod, currentCtx);
                    var builtType = methodParameterTypes[0] with {
                        Qualifier = qualifier
                    };
                    if (builtType.TypeModel != autoBuilderType.TypeModel) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Auto builder method {builderMethod} must accept a first parameter of the auto builder type {autoBuilderType.TypeModel}.",
                            builderMethod.GetLocationOrDefault(),
                            currentCtx);
                    }

                    var parameterTypes = methodParameterTypes
                        .Skip(1)
                        .ToImmutableList();
                    var builderMemberName = builderMethod.Name;

                    return new SpecBuilderMetadata(
                        builtType,
                        builderMemberName,
                        SpecBuilderMemberType.Direct,
                        parameterTypes,
                        builderAttribute,
                        null,
                        builderMethod);
                });
        }

        private bool VerifyExtractBuilder(
            QualifiedTypeModel autoBuilderType,
            IMethodSymbol builderSymbol,
            ExtractorContext? currentCtx) {
            if (!builderAttributeExtractor.CanExtract(builderSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Builder {builderSymbol} must have a {BuilderAttributeMetadata.BuilderAttributeClassName}.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                var qualifier = qualifierExtractor.Extract(builderSymbol, currentCtx).Qualifier;
                if (!Equals(qualifier, autoBuilderType.Qualifier.Qualifier)) {
                    return false;
                }

                if (builderReferenceAttributeExtractor.CanExtract(builderSymbol)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Builder {builderSymbol} cannot have both {BuilderAttributeMetadata.BuilderAttributeClassName} and {BuilderReferenceAttributeMetadata.BuilderReferenceAttributeClassName}.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
                }

                if (builderSymbol is
                    not { DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Builder {builderSymbol} must be a public or internal method.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
                }

                var numMethodParameters = builderSymbol.Parameters.Length;
                if (numMethodParameters == 0) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        "Auto builder must have at least one parameter.",
                        builderSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }
}
