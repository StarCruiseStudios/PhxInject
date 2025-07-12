// -----------------------------------------------------------------------------
// <copyright file="SourceDescriptor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SourceDesc(
    IReadOnlyList<InjectorMetadata> injectorDescs,
    IReadOnlyList<SpecDesc> specDescs,
    IReadOnlyList<DependencyDesc> dependencyDescs
) {
    public IReadOnlyList<SpecDesc> GetAllSpecDescs() {
        return dependencyDescs
            .Select(dep => dep.InstantiatedSpecDesc)
            .Concat(specDescs)
            .ToImmutableList();
    }

    public interface IExtractor {
        SourceDesc Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        private readonly DependencyDesc.IExtractor dependencyDescExtractor;
        private readonly InjectorMetadata.IExtractor injectorDescExtractor;
        private readonly SpecDesc.IExtractor specDescExtractor;
        private readonly SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor;

        public Extractor(
            InjectorMetadata.IExtractor injectorDescExtractor,
            SpecDesc.IExtractor specDescExtractor,
            DependencyDesc.IExtractor dependencyDescExtractor,
            SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor
        ) {
            this.injectorDescExtractor = injectorDescExtractor;
            this.specDescExtractor = specDescExtractor;
            this.dependencyDescExtractor = dependencyDescExtractor;
            this.specificationAttributeExtractor = specificationAttributeExtractor;
        }

        public Extractor() : this(
            InjectorMetadata.Extractor.Instance,
            new SpecDesc.Extractor(),
            new DependencyDesc.Extractor(),
            SpecificationAttributeMetadata.Extractor.Instance
        ) { }

        public SourceDesc Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IGeneratorContext generatorCtx
        ) {
            var extractorCtx = new ExtractorContext(null, null, generatorCtx);

            IReadOnlyList<InjectorMetadata> injectorDescs = injectorCandidates
                .Where(injectorTypeSymbol => injectorDescExtractor.CanExtract(injectorTypeSymbol))
                .SelectCatching(
                    extractorCtx.Aggregator,
                    injectorTypeSymbol => $"extracting injector from {injectorTypeSymbol}",
                    injectorTypeSymbol => injectorDescExtractor.Extract(injectorTypeSymbol, extractorCtx))
                .ToImmutableList();
            extractorCtx.Log($"Discovered {injectorDescs.Count} injector types.");

            IReadOnlyList<DependencyDesc> dependencyDescs = injectorDescs
                .Where(injectorDesc => injectorDesc.DependencyInterfaceType != null)
                .SelectCatching(
                    extractorCtx.Aggregator,
                    injectorDesc => $"extracting dependencies from injector {injectorDesc.InjectorInterfaceType}",
                    injectorDesc => {
                        var dependencySymbol = injectorDesc.DependencyInterfaceType!.TypeSymbol;
                        var injectorType = injectorDesc.InjectorInterfaceType;
                        return dependencyDescExtractor.Extract(dependencySymbol, injectorType, extractorCtx);
                    })
                .ToImmutableList();
            extractorCtx.Log($"Discovered {dependencyDescs.Count} dependency types.");

            IReadOnlyList<SpecDesc> specDescs = specificationCandidates
                .Where(specificationTypeSymbol => specificationAttributeExtractor.CanExtract(specificationTypeSymbol))
                .SelectCatching(
                    extractorCtx.Aggregator,
                    specificationTypeSymbol => $"extracting specification from {specificationTypeSymbol}",
                    specificationTypeSymbol => specDescExtractor.Extract(specificationTypeSymbol, extractorCtx))
                .ToImmutableList();
            extractorCtx.Log($"Discovered {specDescs.Count} specification types.");

            return new SourceDesc(
                injectorDescs,
                specDescs,
                dependencyDescs
            );
        }
    }
}
