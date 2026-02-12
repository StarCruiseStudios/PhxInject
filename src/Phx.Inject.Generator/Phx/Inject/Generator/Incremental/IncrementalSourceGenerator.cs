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
internal static class PhxInject {
    /// <summary> The root namespace for all Phx.Inject types. </summary>
    public const string NamespaceName = "Phx.Inject";
}

/// <summary>
///     Roslyn incremental source generator for compile-time dependency injection.
/// </summary>
/// <param name="metadataPipeline"> The Stage 1 metadata pipeline. </param>
/// <param name="corePipeline"> The Stage 2 core pipeline. </param>
/// <remarks>
///     This generator operates in two stages:
///     <list type="number">
///         <item>
///             <term>Stage 1 (Metadata):</term>
///             <description>Collects and analyzes syntax from source code, producing immutable metadata records.</description>
///         </item>
///         <item>
///             <term>Stage 2 (Core):</term>
///             <description>Transforms metadata into code generation models and produces source code.</description>
///         </item>
///     </list>
///     <seealso
///         href="https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md"/>
/// </remarks>
[Generator(LanguageNames.CSharp)]
internal class IncrementalSourceGenerator(
    MetadataPipeline metadataPipeline,
    CorePipeline corePipeline
) : IIncrementalGenerator {
    /// <summary>
    ///     Initializes a new instance of the <see cref="IncrementalSourceGenerator"/> class with default pipelines.
    /// </summary>
    public IncrementalSourceGenerator() : this(
        MetadataPipeline.Instance,
        CorePipeline.Instance
    ) { }

    /// <summary>
    ///     Initializes the incremental generator and registers source output callbacks.
    /// </summary>
    /// <param name="generatorInitializationContext"> The generator initialization context. </param>
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
