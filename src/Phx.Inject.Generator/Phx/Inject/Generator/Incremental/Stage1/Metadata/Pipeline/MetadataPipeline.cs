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
    ///     </para>
    ///     
    ///     <para>Why Not Parallel Execution?</para>
    ///     <para>
    ///     While this method executes segments sequentially, the actual work (predicate/transform)
    ///     happens lazily during later pipeline stages. This is registration, not execution.
    ///     Roslyn controls the parallel execution internally.
    ///     </para>
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
    /// <remarks>
    ///     Enables pipeline-first syntax: context.Process(pipeline) instead of pipeline.Process(context).
    ///     This reads more naturally in fluent chains and follows Roslyn's extension method conventions.
    /// </remarks>
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
    ///     <para>Purpose:</para>
    ///     <para>
    ///     This method exists solely for generator debugging. It generates synthetic C# files showing
    ///     what metadata was extracted from each declaration. These files appear in IDE tooling,
    ///     allowing developers to inspect the metadata pipeline without debugging.
    ///     </para>
    ///     
    ///     <para>WARNING - Performance Impact:</para>
    ///     <para>
    ///     Calling this method registers dozens of source output callbacks, each of which executes
    ///     for every matching declaration. This significantly increases generation overhead and IDE
    ///     latency. Should NEVER be enabled in production/release builds.
    ///     </para>
    ///     
    ///     <para>When to Use:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Debugging metadata extraction logic during development</description>
    ///         </item>
    ///         <item>
    ///             <description>Validating attribute transformers are capturing correct information</description>
    ///         </item>
    ///         <item>
    ///             <description>Understanding why generated code is incorrect</description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>What Gets Generated:</para>
    ///     <para>
    ///     For each injector, spec, factory, etc., generates a .cs file in the Metadata namespace
    ///     with comments showing extracted providers, activators, factory methods, parameters, etc.
    ///     These files don't compile to useful code - they're structured comments for inspection.
    ///     </para>
    ///     
    ///     <para>Alternative Debugging Approaches:</para>
    ///     <para>
    ///     For production debugging, prefer attaching a debugger to the generator process or adding
    ///     targeted diagnostics. This method is too heavy-weight for anything but deep investigation.
    ///     </para>
    /// </remarks>
    public static MetadataPipelineOutput Print(this MetadataPipelineOutput output, IncrementalGeneratorInitializationContext context) {
        context.RegisterSourceOutput(output.PhxInjectSettingsPipelineSegment,
            (sourceProductionContext, settings) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"/// <remarks>");
                    b.AppendLine($"///     Phx.Inject.Generator: Using settings: {settings}");
                    b.AppendLine($"/// </remarks>");
                    b.AppendLine($"class GeneratorSettings{{ }}");
                });
                sourceProductionContext.AddSource($"Metadata\\GeneratorSettings.cs", source);
            });
        context.RegisterSourceOutput(output.InjectorInterfacePipelineSegment,
            (sourceProductionContext, injector) => {
                var diagnostics = new DiagnosticsRecorder();
                var injectorValue = injector.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injectorValue.InjectorInterfaceType.BaseTypeName} {{");
                    foreach (var provider in injectorValue.Providers) {
                        b.AppendLine(
                            $"  // Provider: {provider.ProvidedType} {provider.ProviderMethodName}");
                    }

                    foreach (var activator in injectorValue.Activators) {
                        b.AppendLine(
                            $"  // Activator: {activator.ActivatedType} {activator.ActivatorMethodName}");
                    }

                    foreach (var childProvider in injectorValue.ChildProviders) {
                        b.Append(
                            $"  // ChildProvider: {childProvider.ChildInjectorType} {childProvider.ChildProviderMethodName}(");
                        b.Append(string.Join(", ", childProvider.Parameters));
                        b.AppendLine(")");
                    }

                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource(
                    $"Metadata\\Generated{injectorValue.InjectorInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.InjectorDependencyPipelineSegment,
            (sourceProductionContext, injectorDependency) => {
                var diagnostics = new DiagnosticsRecorder();
                var injectorDepValue = injectorDependency.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injectorDepValue.InjectorDependencyInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in injectorDepValue.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in injectorDepValue.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{injectorDepValue.InjectorDependencyInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.SpecClassPipelineSegment,
            (sourceProductionContext, specClass) => {
                var diagnostics = new DiagnosticsRecorder();
                var specClassValue = specClass.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{specClassValue.SpecType.BaseTypeName} {{");
                    foreach (var factoryMethod in specClassValue.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in specClassValue.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    foreach (var factoryReference in specClassValue.FactoryReferences) {
                        b.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                        b.Append(string.Join(", ", factoryReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderMethod in specClassValue.BuilderMethods) {
                        b.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                        b.Append(string.Join(", ", builderMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderReference in specClassValue.BuilderReferences) {
                        b.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                        b.Append(string.Join(", ", builderReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var link in specClassValue.Links) {
                        b.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{specClassValue.SpecType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.SpecInterfacePipelineSegment,
            (sourceProductionContext, specInterface) => {
                var diagnostics = new DiagnosticsRecorder();
                var specInterfaceValue = specInterface.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{specInterfaceValue.SpecInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in specInterfaceValue.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in specInterfaceValue.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    foreach (var factoryReference in specInterfaceValue.FactoryReferences) {
                        b.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                        b.Append(string.Join(", ", factoryReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderMethod in specInterfaceValue.BuilderMethods) {
                        b.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                        b.Append(string.Join(", ", builderMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderReference in specInterfaceValue.BuilderReferences) {
                        b.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                        b.Append(string.Join(", ", builderReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var link in specInterfaceValue.Links) {
                        b.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{specInterfaceValue.SpecInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.AutoFactoryPipelineSegment,
            (sourceProductionContext, autoFactory) => {
                var diagnostics = new DiagnosticsRecorder();
                var autoFactoryValue = autoFactory.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoFactoryValue.AutoFactoryType.TypeMetadata.BaseTypeName} {{");
                    b.Append("  // Constructor(");
                    b.Append(string.Join(", ", autoFactoryValue.Parameters));
                    b.AppendLine(")");
                    foreach (var requiredProperty in autoFactoryValue.RequiredProperties) {
                        b.AppendLine($"  // RequiredProperty: {requiredProperty.RequiredPropertyType} {requiredProperty.RequiredPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoFactoryValue.AutoFactoryType.TypeMetadata.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.AutoBuilderPipelineSegment,
            (sourceProductionContext, autoBuilder) => {
                var diagnostics = new DiagnosticsRecorder();
                var autoBuilderValue = autoBuilder.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoBuilderValue.BuiltType.TypeMetadata.BaseTypeName}{autoBuilderValue.AutoBuilderMethodName} {{");
                    b.Append($"  // BuilderMethod: {autoBuilderValue.BuiltType} {autoBuilderValue.AutoBuilderMethodName}(");
                    b.Append(string.Join(", ", autoBuilderValue.Parameters));
                    b.AppendLine(")");
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoBuilderValue.BuiltType.TypeMetadata.NamespacedBaseTypeName}{autoBuilderValue.AutoBuilderMethodName}.cs",
                    source);
            });

        return output;
    }
}