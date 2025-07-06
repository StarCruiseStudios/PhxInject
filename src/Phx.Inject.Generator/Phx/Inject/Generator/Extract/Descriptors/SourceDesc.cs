// -----------------------------------------------------------------------------
// <copyright file="SourceDescriptor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Abstract;

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
        SourceDesc Extract(SourceSyntaxReceiver syntaxReceiver, IExceptionAggregator exceptionAggregator, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        private readonly InjectorDesc.IExtractor injectorDescExtractor;
        private readonly SpecDesc.IExtractor specDescExtractor;
        private readonly DependencyDesc.IExtractor dependencyDescExtractor;

        public Extractor(
            InjectorDesc.IExtractor injectorDescExtractor,
            SpecDesc.IExtractor specDescExtractor,
            DependencyDesc.IExtractor dependencyDescExtractor
        ) {
            this.injectorDescExtractor = injectorDescExtractor;
            this.specDescExtractor = specDescExtractor;
            this.dependencyDescExtractor = dependencyDescExtractor;
        }

        public Extractor() : this(
            new InjectorDesc.Extractor(),
            new SpecDesc.Extractor(),
            new DependencyDesc.Extractor()
        ) { }

        public SourceDesc Extract(SourceSyntaxReceiver syntaxReceiver, IExceptionAggregator exceptionAggregator, IGeneratorContext generatorCtx) {
            var extractorCtx = new ExtractorContext(generatorCtx.ExecutionContext);
            
                IReadOnlyList<InjectorDesc> injectorDescs = syntaxReceiver.InjectorCandidates
                    .SelectCatching(
                        exceptionAggregator,
                        syntaxNode => $"extracting injector from {syntaxNode.Identifier.Text}",
                        syntaxNode => {
                            var injectorSymbol = MetadataHelpers
                                .ExpectTypeSymbolFromDeclaration(syntaxNode, extractorCtx)
                                .GetOrThrow(extractorCtx);
                            return MetadataHelpers.IsInjectorSymbol(injectorSymbol).GetOrThrow(extractorCtx)
                                ? injectorDescExtractor.Extract(injectorSymbol, extractorCtx)
                                : null;
                        })
                    .OfType<InjectorDesc>()
                    .ToImmutableList();
                extractorCtx.Log($"Discovered {injectorDescs.Count} injector types.");

                IReadOnlyList<SpecDesc> specDescs = syntaxReceiver.SpecificationCandidates
                    .SelectCatching(
                        exceptionAggregator,
                        syntaxNode => $"extracting specification from {syntaxNode.Identifier.Text}",
                        syntaxNode => {
                            var specificationSymbol = MetadataHelpers
                                .ExpectTypeSymbolFromDeclaration(syntaxNode, extractorCtx)
                                .GetOrThrow(extractorCtx);
                            return MetadataHelpers.IsSpecSymbol(specificationSymbol).GetOrThrow(extractorCtx)
                                ? specDescExtractor.Extract(specificationSymbol, extractorCtx)
                                : null;
                        })
                    .OfType<SpecDesc>()
                    .ToImmutableList();
                extractorCtx.Log($"Discovered {specDescs.Count} specification types.");

                IReadOnlyList<DependencyDesc> dependencyDescs = syntaxReceiver.InjectorCandidates
                    .SelectCatching(
                        exceptionAggregator,
                        syntaxNode => $"extracting dependencies from injector {syntaxNode.Identifier.Text}",
                        syntaxNode => {
                            var injectorSymbol = MetadataHelpers
                                .ExpectTypeSymbolFromDeclaration(syntaxNode, extractorCtx)
                                .GetOrThrow(extractorCtx);
                            if (!MetadataHelpers.IsInjectorSymbol(injectorSymbol).GetOrThrow(extractorCtx)) {
                                return null;
                            }

                            var dependencySymbol = MetadataHelpers.TryGetDependencyType(injectorSymbol)
                                .GetOrThrow(extractorCtx);
                            if (dependencySymbol == null) {
                                return null;
                            }

                            var injectorType = injectorSymbol.ToTypeModel();
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
