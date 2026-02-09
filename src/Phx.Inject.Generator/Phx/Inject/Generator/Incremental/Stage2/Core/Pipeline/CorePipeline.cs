// -----------------------------------------------------------------------------
// <copyright file="CorePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline;

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline;

internal record CorePipelineOutput(
    MetadataPipelineOutput MetadataPipelineOutput
);

internal class CorePipeline {
    public static readonly CorePipeline Instance = new(
    );
    
    public CorePipelineOutput Process(MetadataPipelineOutput metadataPipeline) {
        return new CorePipelineOutput(metadataPipeline);
    }
}

internal static class MetadataPipelineOutputExtensions {
    public static CorePipelineOutput Process(this MetadataPipelineOutput context, CorePipeline pipeline) {
        return pipeline.Process(context);
    }
}