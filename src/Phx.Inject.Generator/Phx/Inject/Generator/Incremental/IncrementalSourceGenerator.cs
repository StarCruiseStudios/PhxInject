// -----------------------------------------------------------------------------
// <copyright file="IncrementalSourceGenerator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Text;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline;
using Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline;

namespace Phx.Inject.Generator.Incremental;

internal static class PhxInject {
    public const string NamespaceName = "Phx.Inject";
}

/// <summary>
///     <seealso
///         href="https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md"/>
/// </summary>
[Generator(LanguageNames.CSharp)]
internal class IncrementalSourceGenerator(
    MetadataPipeline metadataPipeline,
    CorePipeline corePipeline
) : IIncrementalGenerator {
    public IncrementalSourceGenerator() : this(
        MetadataPipeline.Instance,
        CorePipeline.Instance
    ) { }

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
