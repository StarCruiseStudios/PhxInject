// -----------------------------------------------------------------------------
// <copyright file="MetadataPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Common.Util.StringBuilderUtil;
using static Phx.Inject.Generator.Incremental.Util.EquatableList<Phx.Inject.Generator.Incremental.Diagnostics.DiagnosticInfo>;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline;

/// <summary>
///     Aggregated output from all Stage 1 metadata extraction pipeline segments.
/// </summary>
/// <remarks>
///     Collects parallel metadata extraction results for Stage 2 consumption. Independent segments
///     process different DI declaration types (injectors, specs, auto-factories). Incremental
///     compilation benefit: changes to one file only reprocess affected segments.
/// </remarks>
internal record MetadataPipelineOutput(
    IncrementalValueProvider<IResult<PhxInjectSettingsMetadata>> PhxInjectSettingsPipelineSegment,
    IncrementalValuesProvider<IResult<InjectorInterfaceMetadata>> InjectorInterfacePipelineSegment,
    IncrementalValuesProvider<IResult<InjectorDependencyInterfaceMetadata>> InjectorDependencyPipelineSegment,
    IncrementalValuesProvider<IResult<SpecClassMetadata>> SpecClassPipelineSegment,
    IncrementalValuesProvider<IResult<SpecInterfaceMetadata>> SpecInterfacePipelineSegment,
    IncrementalValuesProvider<IResult<AutoFactoryMetadata>> AutoFactoryPipelineSegment,
    IncrementalValuesProvider<IResult<AutoBuilderMetadata>> AutoBuilderPipelineSegment,
    IncrementalValueProvider<EquatableList<DiagnosticInfo>> DiagnosticsPipelineSegment
);

/// <summary>
///     Stage 1 orchestration layer that coordinates parallel extraction of DI metadata from source code.
/// </summary>
/// <remarks>
///     Two-stage architecture: Stage 1 extracts structural metadata from syntax; Stage 2 transforms
///     to code. Separation enables incremental caching, parallel extraction, centralized validation.
///     Independent segments process different attributes (injectors, specs, factories) in parallel.
///     Diagnostics accumulated in <c>Result</c> wrappers, merged for batch reporting after completion.
/// </remarks>
internal sealed class MetadataPipeline(
    ISyntaxValuePipeline<PhxInjectSettingsMetadata> phxInjectSettingsPipeline,
    ISyntaxValuesPipeline<InjectorInterfaceMetadata> injectorPipeline,
    ISyntaxValuesPipeline<InjectorDependencyInterfaceMetadata> injectorDependencyPipeline,
    ISyntaxValuesPipeline<SpecClassMetadata> specClassPipeline,
    ISyntaxValuesPipeline<SpecInterfaceMetadata> specInterfacePipeline,
    ISyntaxValuesPipeline<AutoFactoryMetadata> autoFactoryPipeline,
    ISyntaxValuesPipeline<AutoBuilderMetadata> autoBuilderPipeline
) {
    /// <summary>
    ///     Gets the singleton pipeline instance configured with production pipeline segments.
    /// </summary>
    /// <remarks>
    ///     Singleton pattern (static configuration). Lightweight structural aggregator.
    /// </remarks>
    public static readonly MetadataPipeline Instance = new(
        PhxInjectSettingsPipeline.Instance,
        InjectorInterfacePipeline.Instance,
        InjectorDependencyPipeline.Instance,
        SpecClassPipeline.Instance,
        SpecInterfacePipeline.Instance,
        AutoFactoryPipeline.Instance,
        AutoBuilderPipeline.Instance
    );
    
    /// <summary>
    ///     Executes all pipeline segments and aggregates their outputs and diagnostics.
    /// </summary>
    /// <param name="generatorInitializationContext">
    ///     Roslyn's generator context providing access to the syntax provider and compilation.
    /// </param>
    /// <returns>
    ///     A MetadataPipelineOutput containing all extracted metadata wrapped in Result&lt;T&gt;
    ///     and a merged diagnostic stream for error reporting.
    /// </returns>
    /// <remarks>
    ///     Each segment registers with <c>SyntaxProvider</c> (predicate + transform). Roslyn executes
    ///     predicates in parallel across syntax nodes, then transforms on passing nodes with caching.
    ///     <c>SelectDiagnostics</c> extracts errors from <c>Result</c> wrappers. Merge combines
    ///     diagnostic streams in binary tree (O(log n) depth vs O(n) linear).
    /// </remarks>
    public MetadataPipelineOutput Process(IncrementalGeneratorInitializationContext generatorInitializationContext) {
        var phxInjectSettingsPipelineSegment =
            phxInjectSettingsPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var injectorInterfacePipelineSegment =
            injectorPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var injectorDependencyPipelineSegment =
            injectorDependencyPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var specClassPipelineSegment = specClassPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var specInterfacePipelineSegment =
            specInterfacePipeline.Select(generatorInitializationContext.SyntaxProvider);
        var autoFactoryPipelineSegment =
            autoFactoryPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var autoBuilderPipelineSegment =
            autoBuilderPipeline.Select(generatorInitializationContext.SyntaxProvider);

        var diagnosticPipelineSegment =
            Merge(
                Merge(
                    Merge(
                        phxInjectSettingsPipelineSegment.SelectDiagnostics(),
                        injectorInterfacePipelineSegment.SelectDiagnostics()
                    ),
                    Merge(
                        injectorDependencyPipelineSegment.SelectDiagnostics(),
                        specClassPipelineSegment.SelectDiagnostics()
                    )
                ),
                Merge(
                    Merge(
                        specInterfacePipelineSegment.SelectDiagnostics(),
                        autoFactoryPipelineSegment.SelectDiagnostics()
                    ),
                    autoBuilderPipelineSegment.SelectDiagnostics()
                )
            );

        return new MetadataPipelineOutput(
            phxInjectSettingsPipelineSegment,
            injectorInterfacePipelineSegment,
            injectorDependencyPipelineSegment,
            specClassPipelineSegment,
            specInterfacePipelineSegment,
            autoFactoryPipelineSegment,
            autoBuilderPipelineSegment,
            diagnosticPipelineSegment
        );
    }
}

/// <summary>
///     Extension methods for integrating MetadataPipeline with Roslyn's generator infrastructure.
/// </summary>
internal static class IncrementalGeneratorInitializationContextExtensions {
    /// <summary>
    ///     Fluent API for executing the metadata pipeline against a generator context.
    /// </summary>
    /// <param name="context">The Roslyn generator initialization context.</param>
    /// <param name="pipeline">The configured metadata pipeline to execute.</param>
    /// <returns>
    ///     MetadataPipelineOutput containing extracted metadata and diagnostics.
    /// </returns>
    public static MetadataPipelineOutput Process(this IncrementalGeneratorInitializationContext context, MetadataPipeline pipeline) {
        return pipeline.Process(context);
    }
    
    /// <summary>
    ///     DEBUG ONLY: Registers source output callbacks that generate diagnostic metadata artifacts.
    /// </summary>
    /// <param name="output">The pipeline output to visualize.</param>
    /// <param name="context">The generator context for registering outputs.</param>
    /// <returns>The unmodified pipeline output for chaining.</returns>
    /// <remarks>
    ///     Generates synthetic C# files showing extracted metadata for debugging. Significant performance
    ///     impact - NEVER enable in production builds. Use for debugging metadata extraction logic,
    ///     validating attribute transformers, or understanding incorrect generated code.
    /// </remarks>
    public static MetadataPipelineOutput Print(this MetadataPipelineOutput output, IncrementalGeneratorInitializationContext context) {
        PhxInjectSettingsPipeline.Instance.Print(context, output.PhxInjectSettingsPipelineSegment);
        InjectorInterfacePipeline.Instance.Print(context, output.InjectorInterfacePipelineSegment);
        InjectorDependencyPipeline.Instance.Print(context, output.InjectorDependencyPipelineSegment);
        SpecClassPipeline.Instance.Print(context, output.SpecClassPipelineSegment);
        SpecInterfacePipeline.Instance.Print(context, output.SpecInterfacePipelineSegment);
        AutoFactoryPipeline.Instance.Print(context, output.AutoFactoryPipelineSegment);
        AutoBuilderPipeline.Instance.Print(context, output.AutoBuilderPipelineSegment);
        return output;
    }
}