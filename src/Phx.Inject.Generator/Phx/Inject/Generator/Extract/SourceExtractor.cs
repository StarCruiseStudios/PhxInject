// -----------------------------------------------------------------------------
// <copyright file="SourceExtractor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Abstract;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract;

internal class SourceExtractor {
    private readonly DependencyDesc.IExtractor dependencyDescExtractor;
    private readonly InjectorDesc.IExtractor injectorDescExtractor;
    private readonly SpecDesc.IExtractor specDescExtractor;

    public SourceExtractor(
        InjectorDesc.IExtractor injectorDescExtractor,
        SpecDesc.IExtractor specDescExtractor,
        DependencyDesc.IExtractor dependencyDescExtractor
    ) {
        this.injectorDescExtractor = injectorDescExtractor;
        this.specDescExtractor = specDescExtractor;
        this.dependencyDescExtractor = dependencyDescExtractor;
    }

    public SourceExtractor() : this(
        new InjectorDesc.Extractor(),
        new SpecDesc.Extractor(),
        new DependencyDesc.Extractor()
    ) { }

    public SourceDesc Extract(SourceSyntaxReceiver syntaxReceiver, IGeneratorContext generatorCtx) {
        return ExceptionAggregator.Try(
            "extracting source descriptors",
            generatorCtx,
            exceptionAggregator => {
                var extractorCtx = new ExtractorContext(generatorCtx.ExecutionContext);

                IReadOnlyList<InjectorDesc> injectorDescs = syntaxReceiver.InjectorCandidates
                    .SelectCatching(
                        exceptionAggregator,
                        syntaxNode => $"extracting injector from {syntaxNode.Identifier.Text}",
                        syntaxNode => {
                            var injectorSymbol = MetadataHelpers
                                .ExpectTypeSymbolFromDeclaration(syntaxNode, extractorCtx)
                                .GetOrThrow(generatorCtx);
                            return TypeHelpers.IsInjectorSymbol(injectorSymbol).GetOrThrow(generatorCtx)
                                ? injectorDescExtractor.Extract(injectorSymbol, extractorCtx)
                                : null;
                        })
                    .OfType<InjectorDesc>()
                    .ToImmutableList();
                generatorCtx.Log($"Discovered {injectorDescs.Count} injector types.");

                IReadOnlyList<SpecDesc> specDescs = syntaxReceiver.SpecificationCandidates
                    .SelectCatching(
                        exceptionAggregator,
                        syntaxNode => $"extracting specification from {syntaxNode.Identifier.Text}",
                        syntaxNode => {
                            var specificationSymbol = MetadataHelpers
                                .ExpectTypeSymbolFromDeclaration(syntaxNode, extractorCtx)
                                .GetOrThrow(generatorCtx);
                            return TypeHelpers.IsSpecSymbol(specificationSymbol).GetOrThrow(generatorCtx)
                                ? specDescExtractor.Extract(specificationSymbol, extractorCtx)
                                : null;
                        })
                    .OfType<SpecDesc>()
                    .ToImmutableList();
                generatorCtx.Log($"Discovered {specDescs.Count} specification types.");

                IReadOnlyList<DependencyDesc> dependencyDescs = syntaxReceiver.InjectorCandidates
                    .SelectCatching(
                        exceptionAggregator,
                        syntaxNode => $"extracting dependencies from injector {syntaxNode.Identifier.Text}",
                        syntaxNode => {
                            var injectorSymbol = MetadataHelpers
                                .ExpectTypeSymbolFromDeclaration(syntaxNode, extractorCtx)
                                .GetOrThrow(generatorCtx);
                            if (!TypeHelpers.IsInjectorSymbol(injectorSymbol).GetOrThrow(generatorCtx)) {
                                return null;
                            }

                            var dependencySymbol = MetadataHelpers.TryGetDependencyType(injectorSymbol)
                                .GetOrThrow(generatorCtx);
                            if (dependencySymbol == null) {
                                return null;
                            }

                            return TypeHelpers.IsDependencySymbol(dependencySymbol).GetOrThrow(generatorCtx)
                                ? dependencyDescExtractor.Extract(dependencySymbol, extractorCtx)
                                : null;
                        })
                    .OfType<DependencyDesc>()
                    .ToImmutableList();
                generatorCtx.Log($"Discovered {dependencyDescs.Count} dependency types.");

                return new SourceDesc(
                    injectorDescs,
                    specDescs,
                    dependencyDescs
                );
            });
    }
}
