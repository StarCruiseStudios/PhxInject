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
///     Output from the core pipeline processing.
/// </summary>
/// <param name="MetadataPipelineOutput"> The metadata pipeline output. </param>
internal record CorePipelineOutput(
    MetadataPipelineOutput MetadataPipelineOutput
);

/// <summary>
///     Pipeline for processing core Stage 2 transformations.
/// </summary>
internal class CorePipeline {
    /// <summary> Gets the singleton instance of the core pipeline. </summary>
    public static readonly CorePipeline Instance = new(
    );
    
    /// <summary>
    ///     Processes the metadata pipeline output.
    /// </summary>
    /// <param name="metadataPipeline"> The metadata pipeline output to process. </param>
    /// <returns> The core pipeline output. </returns>
    public CorePipelineOutput Process(MetadataPipelineOutput metadataPipeline) {
        return new CorePipelineOutput(metadataPipeline);
    }
}

/// <summary>
///     Extension methods for processing metadata pipeline output.
/// </summary>
internal static class MetadataPipelineOutputExtensions {
    /// <summary>
    ///     Processes the metadata pipeline output through the core pipeline.
    /// </summary>
    /// <param name="context"> The metadata pipeline output to process. </param>
    /// <param name="pipeline"> The core pipeline to use. </param>
    /// <returns> The core pipeline output. </returns>
    public static CorePipelineOutput Process(this MetadataPipelineOutput context, CorePipeline pipeline) {
        return pipeline.Process(context);
    }
}