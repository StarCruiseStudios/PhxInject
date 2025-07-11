// -----------------------------------------------------------------------------
//  <copyright file="DependencyProviderDesc.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record DependencyProviderDesc(
    IMethodSymbol Symbol,
    TypeModel DependencyInterface,
    QualifiedTypeModel ProvidedType,
    PartialAttributeMetadata? PartialAttribute
) : IDescriptor {
    public string ProviderMethodName {
        get => Symbol.Name;
    }

    public bool IsPartial {
        get => PartialAttribute != null;
    }

    public Location Location {
        get => Symbol.Locations.First();
    }

    public static void RequireDependencyProvider(
        IMethodSymbol symbol,
        IGeneratorContext generatorCtx
    ) {
        generatorCtx.Aggregator.Aggregate(
            "Validating dependency provider",
            () => {
                if (symbol.ReturnsVoid) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {symbol.Name} must have a return type.",
                        symbol.Locations.First(),
                        generatorCtx);
                }
            },
            () => {
                if (symbol.Parameters.Length > 0) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {symbol.Name} must not have any parameters.",
                        symbol.Locations.First(),
                        generatorCtx);
                }
            });
    }

    public interface IExtractor {
        DependencyProviderDesc Extract(
            IMethodSymbol symbol,
            TypeModel dependencyInterface,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly PartialAttributeMetadata.IExtractor partialAttributeExtractor;
        private readonly QualifierMetadata.IExtractor qualifierExtractor;
        public Extractor(
            PartialAttributeMetadata.IExtractor partialAttributeExtractor,
            QualifierMetadata.IExtractor qualifierExtractor
        ) {
            this.partialAttributeExtractor = partialAttributeExtractor;
            this.qualifierExtractor = qualifierExtractor;
        }

        public Extractor() : this(
            PartialAttributeMetadata.Extractor.Instance,
            QualifierMetadata.Extractor.Instance
        ) { }

        public DependencyProviderDesc Extract(
            IMethodSymbol symbol,
            TypeModel dependencyInterface,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(symbol,
                currentCtx => {
                    RequireDependencyProvider(symbol, currentCtx);
                    var qualifier = qualifierExtractor.Extract(symbol).GetOrThrow(currentCtx);
                    var providedType = symbol.ReturnType.ToQualifiedTypeModel(qualifier);
                    var partialAttribute = partialAttributeExtractor.CanExtract(symbol)
                        ? partialAttributeExtractor.Extract(symbol)
                            .GetOrThrow(currentCtx)
                            .Also(_ => partialAttributeExtractor.ValidateAttributedType(symbol,
                                providedType.TypeModel,
                                currentCtx))
                        : null;

                    return new DependencyProviderDesc(symbol, dependencyInterface, providedType, partialAttribute);
                });
        }
    }
}
