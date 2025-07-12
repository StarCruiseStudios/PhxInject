// -----------------------------------------------------------------------------
//  <copyright file="DependencyProviderMetadata.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record DependencyProviderMetadata(
    TypeModel DependencyInterface,
    string ProviderMethodName,
    QualifiedTypeModel ProvidedType,
    bool IsPartial,
    PartialAttributeMetadata? PartialAttribute,
    IMethodSymbol ProviderMethodSymbol
) : IDescriptor {
    public Location Location {
        get => ProviderMethodSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        DependencyProviderMetadata Extract(
            IMethodSymbol providerMethodSymbol,
            TypeModel dependencyInterface,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor(
        PartialAttributeMetadata.IExtractor partialAttributeExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            PartialAttributeMetadata.Extractor.Instance,
            QualifierMetadata.AttributeExtractor.Instance
        );

        public DependencyProviderMetadata Extract(
            IMethodSymbol providerMethodSymbol,
            TypeModel dependencyInterface,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                $"extracting dependency provider {providerMethodSymbol}",
                providerMethodSymbol,
                currentCtx => {
                    VerifyExtract(providerMethodSymbol, currentCtx);

                    var qualifier = qualifierExtractor.Extract(providerMethodSymbol, currentCtx);
                    var providedType = providerMethodSymbol.ReturnType.ToQualifiedTypeModel(qualifier);
                    var partialAttribute = partialAttributeExtractor.CanExtract(providerMethodSymbol)
                        ? partialAttributeExtractor.Extract(providedType.TypeModel, providerMethodSymbol, currentCtx)
                        : null;
                    var providerMethodName = providerMethodSymbol.Name;
                    var isPartial = partialAttribute != null;

                    return new DependencyProviderMetadata(
                        dependencyInterface,
                        providerMethodName,
                        providedType,
                        isPartial,
                        partialAttribute,
                        providerMethodSymbol);
                });
        }

        private bool VerifyExtract(IMethodSymbol providerMethodSymbol, IGeneratorContext? generatorCtx) {
            if (generatorCtx != null) {
                if (providerMethodSymbol.ReturnsVoid) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {providerMethodSymbol.Name} must have a return type.",
                        providerMethodSymbol.GetLocationOrDefault(),
                        generatorCtx);
                }

                if (providerMethodSymbol.Parameters.Length > 0) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {providerMethodSymbol.Name} must not have any parameters.",
                        providerMethodSymbol.GetLocationOrDefault(),
                        generatorCtx);
                }
            }

            return true;
        }
    }
}
