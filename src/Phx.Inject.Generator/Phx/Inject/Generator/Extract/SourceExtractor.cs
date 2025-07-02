// -----------------------------------------------------------------------------
// <copyright file="SourceExtractor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Generator.Abstract;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract;

internal class SourceExtractor {
    private readonly IInjectorExtractor injectorExtractor;
    private readonly ISpecExtractor specExtractor;
    private readonly IDependencyExtractor dependencyExtractor;

    public SourceExtractor(
        IInjectorExtractor injectorExtractor,
        ISpecExtractor specExtractor,
        IDependencyExtractor dependencyExtractor
    ) {
        this.injectorExtractor = injectorExtractor;
        this.specExtractor = specExtractor;
        this.dependencyExtractor = dependencyExtractor;
    }
    
    public SourceExtractor() : this(
        new InjectorExtractor(),
        new SpecExtractor(),
        new DependencyExtractor()
    ) { }
    
    public SourceDesc Extract(SourceSyntaxReceiver syntaxReceiver, GeneratorExecutionContext context) {
        return InjectionException.Try(context, () => {
            return InjectionException.TryAggregate(context, aggregateException => {
                var descGenerationContext = new DescGenerationContext(context);

                var injectorDescs = aggregateException.Aggregate(context, () => injectorExtractor.Extract(
                    syntaxReceiver.InjectorCandidates,
                    descGenerationContext),
                    ImmutableList.Create<InjectorDesc>);
                context.Log($"Discovered {injectorDescs.Count} injector types.");

                var specDescs = aggregateException.Aggregate(context, () => specExtractor.Extract(
                    syntaxReceiver.SpecificationCandidates,
                    descGenerationContext),
                    ImmutableList.Create<SpecDesc>);
                context.Log($"Discovered {specDescs.Count} specification types.");

                var dependencyDescs = aggregateException.Aggregate(context, () => dependencyExtractor.Extract(
                    syntaxReceiver.InjectorCandidates,
                    descGenerationContext),
                    ImmutableList.Create<DependencyDesc>);
                context.Log($"Discovered {dependencyDescs.Count} dependency types.");

                return new SourceDesc(
                    injectorDescs,
                    specDescs,
                    dependencyDescs
                );
            });
        }, "extracting source descriptors");
    }
}
