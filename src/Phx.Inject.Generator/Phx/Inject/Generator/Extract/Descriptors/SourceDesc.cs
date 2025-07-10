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
            IExceptionAggregator exceptionAggregator,
            IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        private readonly DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor;
        private readonly DependencyDesc.IExtractor dependencyDescExtractor;
        private readonly InjectorAttributeMetadata.IExtractor injectorAttributeExtractor;
        private readonly InjectorDesc.IExtractor injectorDescExtractor;
        private readonly SpecDesc.IExtractor specDescExtractor;

        public Extractor(
            InjectorDesc.IExtractor injectorDescExtractor,
            SpecDesc.IExtractor specDescExtractor,
            DependencyDesc.IExtractor dependencyDescExtractor,
            DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor,
            InjectorAttributeMetadata.IExtractor injectorAttributeExtractor
        ) {
            this.injectorDescExtractor = injectorDescExtractor;
            this.specDescExtractor = specDescExtractor;
            this.dependencyDescExtractor = dependencyDescExtractor;
            this.dependencyAttributeExtractor = dependencyAttributeExtractor;
            this.injectorAttributeExtractor = injectorAttributeExtractor;
        }

        public Extractor() : this(
            new InjectorDesc.Extractor(),
            new SpecDesc.Extractor(),
            new DependencyDesc.Extractor(),
            DependencyAttributeMetadata.Extractor.Instance,
            InjectorAttributeMetadata.Extractor.Instance
        ) { }

        public SourceDesc Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IExceptionAggregator exceptionAggregator,
            IGeneratorContext generatorCtx
        ) {
            var extractorCtx = new ExtractorContext(generatorCtx.ExecutionContext);

            IReadOnlyList<InjectorDesc> injectorDescs = injectorCandidates
                .SelectCatching(
                    exceptionAggregator,
                    injectorTypeSymbol => $"extracting injector from {injectorTypeSymbol}",
                    injectorTypeSymbol => {
                        var injectorAttribute = injectorAttributeExtractor.TryExtract(injectorTypeSymbol, extractorCtx);

                        return injectorAttribute == null
                            ? null
                            : injectorDescExtractor.Extract(injectorTypeSymbol, extractorCtx);
                    })
                .OfType<InjectorDesc>()
                .ToImmutableList();
            extractorCtx.Log($"Discovered {injectorDescs.Count} injector types.");

            IReadOnlyList<SpecDesc> specDescs = specificationCandidates
                .SelectCatching(
                    exceptionAggregator,
                    specificationTypeSymbol => $"extracting specification from {specificationTypeSymbol}",
                    specificationTypeSymbol =>
                        MetadataHelpers.IsSpecSymbol(specificationTypeSymbol).GetOrThrow(extractorCtx)
                            ? specDescExtractor.Extract(specificationTypeSymbol, extractorCtx)
                            : null)
                .OfType<SpecDesc>()
                .ToImmutableList();
            extractorCtx.Log($"Discovered {specDescs.Count} specification types.");

            IReadOnlyList<DependencyDesc> dependencyDescs = injectorCandidates
                .SelectCatching(
                    exceptionAggregator,
                    injectorTypeSymbol => $"extracting dependencies from injector {injectorTypeSymbol}",
                    injectorTypeSymbol => {
                        var injectorAttribute = injectorAttributeExtractor.TryExtract(injectorTypeSymbol, extractorCtx);
                        if (injectorAttribute == null) {
                            return null;
                        }

                        var dependencyAttribute =
                            dependencyAttributeExtractor.TryExtract(injectorTypeSymbol, extractorCtx);
                        if (dependencyAttribute == null) {
                            return null;
                        }

                        var dependencySymbol = dependencyAttribute.DependencyType.TypeSymbol;

                        var injectorType = injectorTypeSymbol.ToTypeModel();
                        return MetadataHelpers.IsDependencySymbol(dependencySymbol).GetOrThrow(extractorCtx)
                            ? dependencyDescExtractor.Extract(dependencySymbol, injectorType, extractorCtx)
                            : null;
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
