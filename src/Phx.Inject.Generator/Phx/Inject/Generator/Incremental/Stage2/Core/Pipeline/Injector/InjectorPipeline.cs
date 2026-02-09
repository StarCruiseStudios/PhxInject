// -----------------------------------------------------------------------------
// <copyright file="InjectorPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Injector;

internal class InjectorPipeline {
    public static readonly InjectorPipeline Instance = new();

    public IncrementalValuesProvider<InjectorModel> Select(
        IncrementalValuesProvider<InjectorInterfaceMetadata> metadataProvider
    ) {
        return metadataProvider.Select((metadata, _) =>
            InjectorMapper.Instance.Map(metadata, constructedSpecifications: []));
    }
}
