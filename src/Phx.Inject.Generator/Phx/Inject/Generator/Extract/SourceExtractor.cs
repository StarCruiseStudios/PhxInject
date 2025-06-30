// -----------------------------------------------------------------------------
// <copyright file="SourceExtractor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

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
        try {
            var descGenerationContext = new DescGenerationContext(context);

            var injectorDescs = injectorExtractor.Extract(
                syntaxReceiver.InjectorCandidates,
                descGenerationContext);
            Logger.Info($"Discovered {injectorDescs.Count} injector types.");

            var specDescs = specExtractor.Extract(
                syntaxReceiver.SpecificationCandidates,
                descGenerationContext);
            Logger.Info($"Discovered {specDescs.Count} specification types.");

            var dependencyDescs = dependencyExtractor.Extract(
                syntaxReceiver.InjectorCandidates,
                descGenerationContext);
            Logger.Info($"Discovered {dependencyDescs.Count} dependency types.");

            return new SourceDesc(
                injectorDescs,
                specDescs,
                dependencyDescs
            );
        } catch (Exception e) {
            var diagnosticData = e is InjectionException ie
                ? ie.DiagnosticData
                : Diagnostics.UnexpectedError;

            throw new InjectionException(
                diagnosticData,
                "An error occurred while extracting source descriptors.",
                Location.None,
                e);
        }
    }
}
