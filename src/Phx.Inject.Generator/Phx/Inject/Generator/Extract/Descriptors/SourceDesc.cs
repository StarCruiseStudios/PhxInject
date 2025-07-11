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
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SourceDesc(
    IReadOnlyList<InjectorDesc> injectorDescs,
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
        private readonly DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor;
        private readonly DependencyDesc.IExtractor dependencyDescExtractor;
        private readonly InjectorAttributeMetadata.IExtractor injectorAttributeExtractor;
        private readonly InjectorDesc.IExtractor injectorDescExtractor;
        private readonly SpecDesc.IExtractor specDescExtractor;
        private readonly SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor;

        public Extractor(
            InjectorDesc.IExtractor injectorDescExtractor,
            SpecDesc.IExtractor specDescExtractor,
            DependencyDesc.IExtractor dependencyDescExtractor,
            DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor,
            InjectorAttributeMetadata.IExtractor injectorAttributeExtractor,
            SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor
        ) {
            this.injectorDescExtractor = injectorDescExtractor;
            this.specDescExtractor = specDescExtractor;
            this.dependencyDescExtractor = dependencyDescExtractor;
            this.dependencyAttributeExtractor = dependencyAttributeExtractor;
            this.injectorAttributeExtractor = injectorAttributeExtractor;
            this.specificationAttributeExtractor = specificationAttributeExtractor;
        }

        public Extractor() : this(
            new InjectorDesc.Extractor(),
            new SpecDesc.Extractor(),
            new DependencyDesc.Extractor(),
            DependencyAttributeMetadata.Extractor.Instance,
            InjectorAttributeMetadata.Extractor.Instance,
            SpecificationAttributeMetadata.Extractor.Instance
        ) { }

        public SourceDesc Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IGeneratorContext generatorCtx
        ) {
            var extractorCtx = new ExtractorContext(null, generatorCtx);

            IReadOnlyList<InjectorDesc> injectorDescs = injectorCandidates
                .Where(injectorTypeSymbol => injectorAttributeExtractor.CanExtract(injectorTypeSymbol))
                .SelectCatching(
                    extractorCtx.Aggregator,
                    injectorTypeSymbol => $"extracting injector from {injectorTypeSymbol}",
                    injectorTypeSymbol => injectorDescExtractor.Extract(injectorTypeSymbol, extractorCtx))
                .ToImmutableList();
            extractorCtx.Log($"Discovered {injectorDescs.Count} injector types.");

            IReadOnlyList<SpecDesc> specDescs = specificationCandidates
                .Where(specificationTypeSymbol => specificationAttributeExtractor.CanExtract(specificationTypeSymbol))
                .SelectCatching(
                    extractorCtx.Aggregator,
                    specificationTypeSymbol => $"extracting specification from {specificationTypeSymbol}",
                    specificationTypeSymbol => specDescExtractor.Extract(specificationTypeSymbol, extractorCtx))
                .ToImmutableList();
            extractorCtx.Log($"Discovered {specDescs.Count} specification types.");

            IReadOnlyList<DependencyDesc> dependencyDescs = injectorCandidates
                .Where(injectorTypeSymbol => injectorAttributeExtractor.CanExtract(injectorTypeSymbol))
                .SelectCatching(
                    extractorCtx.Aggregator,
                    injectorTypeSymbol => $"extracting dependencies from injector {injectorTypeSymbol}",
                    injectorTypeSymbol => {
                        var dependencyAttribute = dependencyAttributeExtractor.CanExtract(injectorTypeSymbol)
                            ? dependencyAttributeExtractor.Extract(injectorTypeSymbol)
                                .GetOrThrow(extractorCtx)
                                .Also(_ => dependencyAttributeExtractor.ValidateAttributedType(injectorTypeSymbol,
                                    extractorCtx))
                            : null;
                        if (dependencyAttribute == null) {
                            return null;
                        }

                        var dependencySymbol = dependencyAttribute.DependencyType.TypeSymbol;

                        var injectorType = injectorTypeSymbol.ToTypeModel();
                        return dependencyDescExtractor.Extract(dependencySymbol, injectorType, extractorCtx);
                    })
                .OfType<DependencyDesc>()
                .ToImmutableList();
            extractorCtx.Log($"Discovered {dependencyDescs.Count} dependency types.");

            return new SourceDesc(
                injectorDescs,
                specDescs,
                dependencyDescs
            );
        }
    }
}
