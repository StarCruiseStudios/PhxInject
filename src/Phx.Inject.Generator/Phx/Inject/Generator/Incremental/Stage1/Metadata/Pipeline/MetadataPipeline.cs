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
///     <para>Purpose:</para>
///     <para>
///     Collects the results of parallel metadata extraction pipelines into a single data structure
///     for consumption by Stage 2 (core pipeline). Each segment processes a different category of
///     DI declarations (injectors, specs, auto-factories, etc.) independently, leveraging Roslyn's
///     incremental compilation infrastructure for optimal performance.
///     </para>
///     
///     <para>Incremental Compilation Benefits:</para>
///     <para>
///     By separating extraction into independent pipeline segments, changes to one file only
///     reprocess the affected segments. For example, modifying a specification class doesn't
///     re-extract injector metadata from unchanged files. The DiagnosticsPipelineSegment aggregates
///     all diagnostics for batch reporting after extraction completes.
///     </para>
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
///     <para>Architectural Role - Two-Stage Processing:</para>
///     <para>
///     MetadataPipeline is the first of two major pipeline stages in the generator architecture.
///     Stage 1 (this class) extracts structural metadata from source syntax, while Stage 2 (CorePipeline)
///     transforms that metadata into executable implementation code. This separation enables:
///     </para>
///     <list type="number">
///         <item>
///             <description>
///             Incremental caching at metadata level - syntax changes don't invalidate code generation
///             </description>
///         </item>
///         <item>
///             <description>
///             Parallel extraction of independent declaration types (injectors, specs, factories)
///             </description>
///         </item>
///         <item>
///             <description>
///             Centralized validation and error reporting before attempting code generation
///             </description>
///         </item>
///     </list>
///     
///     <para>Pipeline Segment Independence:</para>
///     <para>
///     Each segment processes a specific attribute-marked declaration type independently:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>PhxInjectSettingsPipeline:</term>
///             <description>Singleton assembly-level configuration</description>
///         </item>
///         <item>
///             <term>InjectorInterfacePipeline:</term>
///             <description>@Injector interface declarations that define DI containers</description>
///         </item>
///         <item>
///             <term>InjectorDependencyPipeline:</term>
///             <description>@InjectorDependency interfaces consumed by child injectors</description>
///         </item>
///         <item>
///             <term>SpecClassPipeline/SpecInterfacePipeline:</term>
///             <description>@Specification types that define factory/builder methods</description>
///         </item>
///         <item>
///             <term>AutoFactoryPipeline/AutoBuilderPipeline:</term>
///             <description>@AutoFactory/@AutoBuilder types for automatic code generation</description>
///         </item>
///     </list>
///     
///     <para>Diagnostic Aggregation Strategy:</para>
///     <para>
///     Instead of reporting errors during extraction, each segment accumulates diagnostics in
///     Result&lt;T&gt; wrappers. The Process method merges all diagnostics into a single stream
///     for batch reporting after all segments complete. This prevents error avalanches where one
///     malformed declaration causes cascade failures in dependent segments.
///     </para>
///     
///     <para>Performance Characteristics:</para>
///     <para>
///     Leverages Roslyn's parallel processing via IncrementalValuesProvider. Each segment's
///     predicate filter executes on syntax nodes in parallel, with transformations cached between
///     compilations. Typically only 1-5% of syntax nodes pass predicate filters, making the
///     transform phase extremely targeted.
///     </para>
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
    ///     Uses singleton pattern since pipeline configuration is static. All segments are themselves
    ///     singletons, making this a lightweight structural aggregator rather than a stateful service.
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
    ///     <para>Execution Flow:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///             Each segment's Select method registers with SyntaxProvider (predicate + transform)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Roslyn executes predicates in parallel across all syntax nodes
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Transforms execute for nodes that pass predicate, with incremental caching
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             SelectDiagnostics extracts error/warning info from Result wrappers
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Merge combines diagnostic streams into single aggregated output
    ///             </description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>Diagnostic Merging Tree:</para>
    ///     <para>
    ///     Diagnostics are merged in a binary tree structure to balance merge operations and
    ///     avoid long linear chains that could create performance bottlenecks or stack depth issues.
    ///     The tree structure ensures O(log n) merge depth instead of O(n).
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
        var diagnostics = new DiagnosticsRecorder();
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
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injector.GetValue(diagnostics).InjectorInterfaceType.BaseTypeName} {{");
                    foreach (var provider in injector.GetValue(diagnostics).Providers) {
                        b.AppendLine(
                            $"  // Provider: {provider.ProvidedType} {provider.ProviderMethodName}");
                    }

                    foreach (var activator in injector.GetValue(diagnostics).Activators) {
                        b.AppendLine(
                            $"  // Activator: {activator.ActivatedType} {activator.ActivatorMethodName}");
                    }

                    foreach (var childProvider in injector.GetValue(diagnostics).ChildProviders) {
                        b.Append(
                            $"  // ChildProvider: {childProvider.ChildInjectorType} {childProvider.ChildProviderMethodName}(");
                        b.Append(string.Join(", ", childProvider.Parameters));
                        b.AppendLine(")");
                    }

                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource(
                    $"Metadata\\Generated{injector.GetValue(diagnostics).InjectorInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.InjectorDependencyPipelineSegment,
            (sourceProductionContext, injectorDependency) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injectorDependency.GetValue(diagnostics).InjectorDependencyInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in injectorDependency.GetValue(diagnostics).FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in injectorDependency.GetValue(diagnostics).FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{injectorDependency.GetValue(diagnostics).InjectorDependencyInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.SpecClassPipelineSegment,
            (sourceProductionContext, specClass) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{specClass.GetValue(diagnostics).SpecType.BaseTypeName} {{");
                    foreach (var factoryMethod in specClass.GetValue(diagnostics).FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in specClass.GetValue(diagnostics).FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    foreach (var factoryReference in specClass.GetValue(diagnostics).FactoryReferences) {
                        b.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                        b.Append(string.Join(", ", factoryReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderMethod in specClass.GetValue(diagnostics).BuilderMethods) {
                        b.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                        b.Append(string.Join(", ", builderMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderReference in specClass.GetValue(diagnostics).BuilderReferences) {
                        b.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                        b.Append(string.Join(", ", builderReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var link in specClass.GetValue(diagnostics).Links) {
                        b.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{specClass.GetValue(diagnostics).SpecType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.SpecInterfacePipelineSegment,
            (sourceProductionContext, specInterface) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{specInterface.GetValue(diagnostics).SpecInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in specInterface.GetValue(diagnostics).FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in specInterface.GetValue(diagnostics).FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    foreach (var factoryReference in specInterface.GetValue(diagnostics).FactoryReferences) {
                        b.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                        b.Append(string.Join(", ", factoryReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderMethod in specInterface.GetValue(diagnostics).BuilderMethods) {
                        b.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                        b.Append(string.Join(", ", builderMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderReference in specInterface.GetValue(diagnostics).BuilderReferences) {
                        b.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                        b.Append(string.Join(", ", builderReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var link in specInterface.GetValue(diagnostics).Links) {
                        b.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{specInterface.GetValue(diagnostics).SpecInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.AutoFactoryPipelineSegment,
            (sourceProductionContext, autoFactory) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoFactory.GetValue(diagnostics).AutoFactoryType.TypeMetadata.BaseTypeName} {{");
                    b.Append("  // Constructor(");
                    b.Append(string.Join(", ", autoFactory.GetValue(diagnostics).Parameters));
                    b.AppendLine(")");
                    foreach (var requiredProperty in autoFactory.GetValue(diagnostics).RequiredProperties) {
                        b.AppendLine($"  // RequiredProperty: {requiredProperty.RequiredPropertyType} {requiredProperty.RequiredPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoFactory.GetValue(diagnostics).AutoFactoryType.TypeMetadata.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.AutoBuilderPipelineSegment,
            (sourceProductionContext, autoBuilder) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoBuilder.GetValue(diagnostics).BuiltType.TypeMetadata.BaseTypeName}{autoBuilder.GetValue(diagnostics).AutoBuilderMethodName} {{");
                    b.Append($"  // BuilderMethod: {autoBuilder.GetValue(diagnostics).BuiltType} {autoBuilder.GetValue(diagnostics).AutoBuilderMethodName}(");
                    b.Append(string.Join(", ", autoBuilder.GetValue(diagnostics).Parameters));
                    b.AppendLine(")");
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoBuilder.GetValue(diagnostics).BuiltType.TypeMetadata.NamespacedBaseTypeName}{autoBuilder.GetValue(diagnostics).AutoBuilderMethodName}.cs",
                    source);
            });

        return output;
    }
}