// -----------------------------------------------------------------------------
// <copyright file="IncrementalSourceGenerator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline;
using Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline;

#endregion

namespace Phx.Inject.Generator.Incremental;

/// <summary>
///     Constants for the Phx.Inject namespace and framework.
/// </summary>
/// <remarks>
///     Centralizes framework namespace constants to ensure consistency across generated code
///     and avoid magic strings scattered throughout the generator codebase.
/// </remarks>
internal static class PhxInject {
    /// <summary>
    ///     The root namespace for all Phx.Inject types.
    /// </summary>
    /// <remarks>
    ///     Used by transformers and code generators to construct fully-qualified type names
    ///     when referencing framework types in generated code.
    /// </remarks>
    public const string NamespaceName = "Phx.Inject";
}

/// <summary>
///     Roslyn incremental source generator that performs compile-time dependency injection
///     by analyzing DI specifications and generating zero-overhead injection code.
/// </summary>
/// <param name="metadataPipeline">
///     Stage 1 pipeline that extracts and validates DI metadata from user code.
/// </param>
/// <param name="corePipeline">
///     Stage 2 pipeline that transforms metadata into executable injection implementations.
/// </param>
/// <remarks>
///     <para>Design Philosophy:</para>
///     <para>
///     This generator moves dependency resolution from runtime to compile-time, eliminating
///     reflection overhead and detecting configuration errors during build. Unlike traditional
///     DI containers, this approach produces AOT-friendly, debuggable code with zero runtime cost.
///     </para>
///     
///     <para>Two-Stage Architecture:</para>
///     <list type="number">
///         <item>
///             <term>Stage 1 (Metadata):</term>
///             <description>
///             Parses source code to extract DI specifications (factories, builders, injectors).
///             Produces immutable metadata records that leverage Roslyn's incremental compilation
///             model to minimize recomputation when unrelated code changes.
///             </description>
///         </item>
///         <item>
///             <term>Stage 2 (Core):</term>
///             <description>
///             Transforms validated metadata into concrete C# code that implements the dependency
///             graph. Generated code is statically typed, fully inlinable, and produces no
///             allocations beyond the injected objects themselves.
///             </description>
///         </item>
///     </list>
///     
///     <para>Incremental Generation:</para>
///     <para>
///     Designed to work efficiently with Roslyn's incremental generator model. Each pipeline
///     stage produces cacheable, value-comparable results. When a file changes, only affected
///     pipelines re-execute, dramatically improving IDE responsiveness in large solutions.
///     </para>
///     
///     <para>Thread Safety:</para>
///     <para>
///     Generator instances may be shared across builds. All state is immutable or thread-local.
///     Pipelines are singleton instances that can safely process multiple syntax trees concurrently.
///     </para>
///     
///     <seealso href="https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md"/>
/// </remarks>
[Generator(LanguageNames.CSharp)]
internal sealed class IncrementalSourceGenerator(
    MetadataPipeline metadataPipeline,
    CorePipeline corePipeline
) : IIncrementalGenerator {
    /// <summary>
    ///     Initializes a new instance with default singleton pipelines for production use.
    /// </summary>
    /// <remarks>
    ///     This is the constructor invoked by Roslyn when instantiating the generator.
    ///     Uses singleton pipeline instances to ensure consistent behavior and avoid
    ///     redundant allocations across multiple generator invocations.
    /// </remarks>
    public IncrementalSourceGenerator() : this(
        MetadataPipeline.Instance,
        CorePipeline.Instance
    ) { }

    /// <summary>
    ///     Registers incremental transformation pipelines and source output callbacks with Roslyn.
    /// </summary>
    /// <param name="generatorInitializationContext">
    ///     The Roslyn context providing access to syntax providers and output registration.
    /// </param>
    /// <remarks>
    ///     <para>
    ///     This method defines the entire incremental pipeline graph. Roslyn caches intermediate
    ///     results between builds, only re-executing nodes when their inputs change. This is
    ///     critical for maintaining fast incremental compilation in large projects.
    ///     </para>
    ///     
    ///     <para>Pipeline Flow:</para>
    ///     <list type="number">
    ///         <item>Process metadata from syntax trees (Stage 1)</item>
    ///         <item>Emit debug representations of parsed metadata (if enabled)</item>
    ///         <item>Transform metadata into code generation models (Stage 2)</item>
    ///         <item>Register diagnostic output to surface errors to the IDE/compiler</item>
    ///     </list>
    ///     
    ///     <para>
    ///     Diagnostics are emitted as a separate output stream to ensure errors appear
    ///     even if code generation fails partway through.
    ///     </para>
    /// </remarks>
    public void Initialize(IncrementalGeneratorInitializationContext generatorInitializationContext) {
        var output = generatorInitializationContext
            .Process(metadataPipeline)
            .Print(generatorInitializationContext)
            .Process(corePipeline);
        generatorInitializationContext.RegisterSourceOutput(output.MetadataPipelineOutput.DiagnosticsPipelineSegment,
            (context, diagnostics) => {
                foreach (var diagnosticInfo in diagnostics) {
                    diagnosticInfo.Report(context);
                }
            });
        // generatorInitializationContext.RegisterSourceOutput(injectorPipeline.Combine(phxInjectSettingsPipeline),
        //     (sourceProductionContext, pair) => {
        //         var injector = pair.Left;
        //         var settings = pair.Right;
        //         
        //         sourceProductionContext.AddSource($"{injector.InjectorInterfaceType.NamespacedBaseTypeName}.settings.cs",
        //             $"/// <remarks>\n" +
        //             $"///     Phx.Inject.Generator: Using settings: {settings}\n" +
        //             $"/// </remarks>\n" +
        //             $"class Generated{injector.InjectorInterfaceType.BaseTypeName} {{ }}");
        //         sourceProductionContext.ReportDiagnostic(Diagnostics.DebugMessage.CreateDiagnostic(
        //             $"Phx.Inject.Generator: Using settings: {settings}",
        //             settings.Location.Value));
        //     });
    }
}
