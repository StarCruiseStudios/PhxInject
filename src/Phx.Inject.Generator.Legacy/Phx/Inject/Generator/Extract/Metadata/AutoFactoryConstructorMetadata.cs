// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryConstructorMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record AutoFactoryConstructorMetadata(
    IMethodSymbol ConstructorSymbol,
    IReadOnlyList<QualifiedTypeModel> ParameterTypes,
    IReadOnlyList<AutoFactoryRequiredPropertyMetadata> RequiredProperties,
    ITypeSymbol AutoFactoryType
) : IMetadata {
    public Location Location {
        get => AutoFactoryType.GetLocationOrDefault();
    }

    internal interface IExtractor {
        bool CanExtract(ITypeSymbol autoFactoryType);
        AutoFactoryConstructorMetadata Extract(
            ITypeSymbol autoFactoryType,
            ExtractorContext parentCtx);
    }

    internal class Extractor(
        AutoFactoryRequiredPropertyMetadata.IExtractor requiredPropertyExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance =
            new Extractor(
                AutoFactoryRequiredPropertyMetadata.Extractor.Instance,
                QualifierMetadata.AttributeExtractor.Instance
            );

        public bool CanExtract(ITypeSymbol autoFactoryType) {
            return VerifyExtract(autoFactoryType, null);
        }

        public AutoFactoryConstructorMetadata Extract(
            ITypeSymbol autoFactoryType,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting auto factory constructor for type {autoFactoryType}",
                autoFactoryType,
                currentCtx => {
                    VerifyExtract(autoFactoryType, currentCtx);
                    IReadOnlyList<IMethodSymbol> constructors = autoFactoryType
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(m => m is {
                            MethodKind: MethodKind.Constructor,
                            DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                        })
                        .ToImmutableList();

                    if (constructors.Count != 1) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Auto factory type '{autoFactoryType}' must contain exactly one public or internal constructor",
                            autoFactoryType.GetLocationOrDefault(),
                            currentCtx);
                    }

                    var constructorMethod = constructors.Single();
                    IReadOnlyList<QualifiedTypeModel> constructorParameterTypes = constructorMethod.Parameters
                        .Select(parameter => {
                            var qualifier = qualifierExtractor.Extract(parameter, currentCtx);
                            return parameter.Type.ToQualifiedTypeModel(qualifier);
                        })
                        .ToImmutableList();

                    IReadOnlyList<AutoFactoryRequiredPropertyMetadata> requiredProperties = autoFactoryType
                        .GetMembers()
                        .OfType<IPropertySymbol>()
                        .Where(requiredPropertyExtractor.CanExtract)
                        .Select(propertySymbol => requiredPropertyExtractor.Extract(propertySymbol, currentCtx))
                        .ToImmutableList();

                    return new AutoFactoryConstructorMetadata(
                        constructorMethod,
                        constructorParameterTypes,
                        requiredProperties,
                        autoFactoryType
                    );
                });
        }

        private bool VerifyExtract(ITypeSymbol autoFactoryType, ExtractorContext? currentCtx) {
            if (autoFactoryType is not {
                    IsStatic: false,
                    IsAbstract: false,
                    TypeKind: TypeKind.Class,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InvalidSpecification.AsException(
                        $"Auto factory type {autoFactoryType} must be a non-static, non-abstract, public or internal class.",
                        autoFactoryType.GetLocationOrDefault(),
                        currentCtx);
            }

            if (autoFactoryType is not INamedTypeSymbol namedTypeSymbol
                || !namedTypeSymbol.TypeArguments.IsEmpty
            ) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InvalidSpecification.AsException(
                        $"Auto factory type {autoFactoryType} must be a non-generic type.",
                        autoFactoryType.GetLocationOrDefault(),
                        currentCtx);
            }

            return true;
        }
    }
}
