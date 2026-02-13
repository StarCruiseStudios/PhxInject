// -----------------------------------------------------------------------------
// <copyright file="CorePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline;

/// <summary>
///     Stage 2 pipeline output containing both metadata and generated implementation artifacts.
/// </summary>
/// <param name="MetadataPipelineOutput">
///     The Stage 1 metadata that serves as input to Stage 2 transformations.
/// </param>
/// <remarks>
///     <para>Current State - Pass-Through Architecture:</para>
///     <para>
///     Currently, CorePipeline is a structural placeholder that passes Stage 1 output through
///     unchanged. The actual implementation code generation happens in the rendering stage, not
///     in this pipeline. This design allows for future expansion where Stage 2 could perform
///     additional transformations (dependency graph analysis, optimization passes, etc.) before
///     final code generation.
///     </para>
///     
///     <para>Future Enhancement Opportunities:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Dependency graph construction and cycle detection
///             </description>
///         </item>
///         <item>
///             <description>
///             Cross-specification validation (ensure all dependencies can be satisfied)
///             </description>
///         </item>
///         <item>
///             <description>
///             Optimization passes (dead code elimination, inline candidates)
///             </description>
///         </item>
///         <item>
///             <description>
///             Code generation IR (intermediate representation before string rendering)
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record CorePipelineOutput(
    MetadataPipelineOutput MetadataPipelineOutput
);

/// <summary>
///     Stage 2 pipeline responsible for transforming validated metadata into implementation models.
/// </summary>
/// <remarks>
///     <para>Architectural Position - Two-Stage Design:</para>
///     <para>
///     CorePipeline is Stage 2 of the generator's two-stage architecture:
///     </para>
///     <list type="number">
///         <item>
///             <term>Stage 1 (MetadataPipeline):</term>
///             <description>
///             Extracts and validates metadata from source syntax. Focuses on "what exists" in user code.
///             </description>
///         </item>
///         <item>
///             <term>Stage 2 (CorePipeline):</term>
///             <description>
///             Transforms metadata into implementation models. Focuses on "what to generate."
///             </description>
///         </item>
///     </list>
///     
///     <para>Why Separate Stages?</para>
///     <list type="bullet">
///         <item>
///             <term>Incremental Compilation Optimization:</term>
///             <description>
///             Changes to implementation strategy (how code is generated) don't invalidate metadata
///             extraction. Only syntax changes trigger Stage 1 re-execution.
///             </description>
///         </item>
///         <item>
///             <term>Clear Separation of Concerns:</term>
///             <description>
///             Stage 1 deals with Roslyn symbols and syntax. Stage 2 deals with domain models and
///             code generation strategy. No mixing of concerns.
///             </description>
///         </item>
///         <item>
///             <term>Testability:</term>
///             <description>
///             Can test metadata extraction and code generation independently with different strategies.
///             </description>
///         </item>
///         <item>
///             <term>Error Isolation:</term>
///             <description>
///             All validation errors caught in Stage 1 prevent Stage 2 from attempting to generate
///             invalid code. No defensive programming needed in generators.
///             </description>
///         </item>
///     </list>
///     
///     <para>Current Implementation - Structural Placeholder:</para>
///     <para>
///     Currently, this class is a pass-through that returns metadata unchanged. The actual
///     transformation to implementation models happens during source output registration, not
///     in this pipeline. This design anticipates future needs without adding complexity now.
///     </para>
///     
///     <para>Future Direction:</para>
///     <para>
///     As the generator evolves, Stage 2 could perform whole-program analysis that requires
///     seeing all metadata at once (e.g., building a dependency graph across all specs, detecting
///     circular dependencies, optimizing away unnecessary factories). The two-stage split provides
///     a natural insertion point for these features without refactoring the architecture.
///     </para>
/// </remarks>
internal class CorePipeline {
    /// <summary>
    ///     Gets the singleton pipeline instance.
    /// </summary>
    /// <remarks>
    ///     Uses singleton pattern since the pipeline has no configuration or state.
    ///     Future versions may accept configuration through the constructor.
    /// </remarks>
    public static readonly CorePipeline Instance = new(
    );
    
    /// <summary>
    ///     Transforms Stage 1 metadata into Stage 2 implementation models.
    /// </summary>
    /// <param name="metadataPipeline">
    ///     The extracted and validated metadata from Stage 1 processing.
    /// </param>
    /// <returns>
    ///     CorePipelineOutput containing implementation models ready for code generation.
    ///     Currently, this is just the input metadata wrapped unchanged.
    /// </returns>
    /// <remarks>
    ///     <para>Current Behavior - Pass-Through:</para>
    ///     <para>
    ///     Simply wraps the input in CorePipelineOutput without transformation. This maintains
    ///     the two-stage architecture boundary while deferring actual transformation logic to
    ///     the source output phase.
    ///     </para>
    ///     
    ///     <para>Why Not Do Transformation Here?</para>
    ///     <para>
    ///     Current transformation is 1:1 (each metadata element maps directly to generated code).
    ///     When we need M:N transformations or whole-program analysis, this method becomes the
    ///     natural place to implement that logic.
    ///     </para>
    /// </remarks>
    public CorePipelineOutput Process(MetadataPipelineOutput metadataPipeline) {
        return new CorePipelineOutput(metadataPipeline);
    }
}

/// <summary>
///     Extension methods for fluent pipeline composition between stages.
/// </summary>
internal static class MetadataPipelineOutputExtensions {
    /// <summary>
    ///     Fluent API for passing Stage 1 output into Stage 2 processing.
    /// </summary>
    /// <param name="context">The metadata pipeline output from Stage 1.</param>
    /// <param name="pipeline">The Stage 2 pipeline to execute.</param>
    /// <returns>
    ///     CorePipelineOutput containing implementation models for code generation.
    /// </returns>
    /// <remarks>
    ///     Enables fluent pipeline chaining: stage1Output.Process(corePipeline).
    ///     This reads naturally in the generator's Initialize method where stages execute sequentially.
    /// </remarks>
    public static CorePipelineOutput Process(this MetadataPipelineOutput context, CorePipeline pipeline) {
        return pipeline.Process(context);
    }
}