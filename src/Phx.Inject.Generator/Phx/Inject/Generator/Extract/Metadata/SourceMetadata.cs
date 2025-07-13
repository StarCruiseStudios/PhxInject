// -----------------------------------------------------------------------------
// <copyright file="SourceMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record SourceMetadata(
    IReadOnlyList<InjectorMetadata> injectorMetadata,
    IReadOnlyList<SpecMetadata> specMetadata,
    IReadOnlyList<DependencyMetadata> dependencyMetadata
) {
    public interface IExtractor {
        SourceMetadata Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IGeneratorContext parentCtx);
    }

    public class Extractor(
        InjectorMetadata.IExtractor injectorExtractor,
        DependencyMetadata.IExtractor dependencyExtractor,
        SpecMetadata.IExtractor specExtractor,
        SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            InjectorMetadata.Extractor.Instance,
            DependencyMetadata.Extractor.Instance,
            SpecMetadata.Extractor.Instance,
            SpecificationAttributeMetadata.Extractor.Instance
        );

        public SourceMetadata Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IGeneratorContext parentCtx
        ) {
            return ExtractorContext.CreateExtractorContext(
                parentCtx,
                currentCtx => {
                    IReadOnlyList<InjectorMetadata> injectorMetadata = injectorCandidates
                        .Where(injectorExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            injectorTypeSymbol => $"extracting injector from {injectorTypeSymbol}",
                            injectorTypeSymbol => injectorExtractor.Extract(injectorTypeSymbol, currentCtx))
                        .ToImmutableList();
                    currentCtx.Log($"Discovered {injectorMetadata.Count} injector types.");

                    IReadOnlyList<DependencyMetadata> dependencyMetadata = injectorMetadata
                        .Where(injectorMetadata => injectorMetadata.DependencyInterfaceType != null)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            injectorMetadata =>
                                $"extracting dependencies from injector {injectorMetadata.InjectorInterfaceType}",
                            injectorMetadata => {
                                var dependencySymbol = injectorMetadata.DependencyInterfaceType!.TypeSymbol;
                                var injectorType = injectorMetadata.InjectorInterfaceType;
                                return dependencyExtractor.Extract(dependencySymbol, injectorType, currentCtx);
                            })
                        .ToImmutableList();
                    currentCtx.Log($"Discovered {dependencyMetadata.Count} dependency types.");

                    IReadOnlyList<SpecMetadata> specMetadata = specificationCandidates
                        .Where(specificationAttributeExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            specificationTypeSymbol => $"extracting specification from {specificationTypeSymbol}",
                            specificationTypeSymbol => specExtractor.Extract(specificationTypeSymbol, currentCtx))
                        .ToImmutableList();
                    currentCtx.Log($"Discovered {specMetadata.Count} specification types.");

                    return new SourceMetadata(
                        injectorMetadata,
                        specMetadata,
                        dependencyMetadata
                    );
                });
        }
    }
}
