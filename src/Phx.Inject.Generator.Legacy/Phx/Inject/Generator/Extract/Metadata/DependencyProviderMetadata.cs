// -----------------------------------------------------------------------------
// <copyright file="DependencyProviderMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record DependencyProviderMetadata(
    QualifiedTypeModel ProvidedType,
    string ProviderName,
    DependencyProviderMemberType ProviderMemberType,
    bool IsPartial,
    PartialAttributeMetadata? PartialAttributeMetadata,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    ISymbol ProviderSymbol
) : IMetadata {
    public Location Location {
        get => ProviderSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        bool CanExtract(ISymbol providerSymbol);
        DependencyProviderMetadata Extract(
            ISymbol providerSymbol,
            ExtractorContext parentCtx);
    }

    public class Extractor(
        FactoryAttributeMetadata.IExtractor factoryAttributeExtractor,
        PartialAttributeMetadata.IExtractor partialAttributeExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            FactoryAttributeMetadata.Extractor.Instance,
            PartialAttributeMetadata.Extractor.Instance,
            QualifierMetadata.AttributeExtractor.Instance
        );

        public bool CanExtract(ISymbol providerSymbol) {
            return VerifyExtract(providerSymbol, null);
        }

        public DependencyProviderMetadata Extract(
            ISymbol providerSymbol,
            ExtractorContext parentCtx) {
            return parentCtx.UseChildExtractorContext(
                $"extracting dependency provider {providerSymbol}",
                providerSymbol,
                currentCtx => {
                    VerifyExtract(providerSymbol, currentCtx);

                    var factoryAttribute = factoryAttributeExtractor.ExtractFactory(providerSymbol, currentCtx);
                    var (providerMemberType, returnTypeSymbol) = providerSymbol switch {
                        IMethodSymbol methodSymbol => (DependencyProviderMemberType.Method, methodSymbol.ReturnType),
                        IPropertySymbol propertySymbol => (DependencyProviderMemberType.Property, propertySymbol.Type),
                        _ => throw Diagnostics.InvalidSpecification.AsException(
                            $"Dependency provider {providerSymbol.Name} must be a method or property.",
                            providerSymbol.GetLocationOrDefault(),
                            currentCtx)
                    };

                    var qualifier = qualifierExtractor.Extract(providerSymbol, currentCtx);
                    var providedType = returnTypeSymbol.ToQualifiedTypeModel(qualifier);
                    var partialAttribute = partialAttributeExtractor.CanExtract(providerSymbol)
                        ? partialAttributeExtractor.Extract(providedType.TypeModel, providerSymbol, currentCtx)
                        : null;
                    var providerName = providerSymbol.Name;
                    var isPartial = partialAttribute != null;

                    return new DependencyProviderMetadata(
                        providedType,
                        providerName,
                        providerMemberType,
                        isPartial,
                        partialAttribute,
                        factoryAttribute,
                        providerSymbol);
                });
        }

        private bool VerifyExtract(ISymbol providerSymbol, IGeneratorContext? currentCtx) {
            if (providerSymbol is not IMethodSymbol and not IPropertySymbol) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {providerSymbol.Name} must be a property or method.",
                        providerSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (!factoryAttributeExtractor.CanExtract(providerSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {providerSymbol.Name} must have a {FactoryAttributeMetadata.FactoryAttributeClassName}.",
                        providerSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (providerSymbol is IMethodSymbol providerMethodSymbol) {
                    if (providerMethodSymbol.ReturnsVoid) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Dependency provider {providerSymbol.Name} must have a return type.",
                            providerSymbol.GetLocationOrDefault(),
                            currentCtx);
                    }

                    if (providerMethodSymbol.Parameters.Length > 0) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Dependency provider {providerSymbol.Name} must not have any parameters.",
                            providerSymbol.GetLocationOrDefault(),
                            currentCtx);
                    }
                }
            }

            return true;
        }
    }
}
