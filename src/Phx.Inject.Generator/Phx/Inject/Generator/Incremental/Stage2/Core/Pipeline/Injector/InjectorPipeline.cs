// -----------------------------------------------------------------------------
// <copyright file="InjectorPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Injector;

/// <summary>
///     Pipeline for transforming injector interface metadata into injector models.
/// </summary>
internal class InjectorPipeline {
    /// <summary> Gets the singleton instance of the injector pipeline. </summary>
    public static readonly InjectorPipeline Instance = new();

    /// <summary>
    ///     Transforms injector interface metadata into injector models.
    /// </summary>
    /// <param name="metadataProvider"> Provider for injector interface metadata. </param>
    /// <returns> Provider of injector models. </returns>
    public IncrementalValuesProvider<InjectorModel> Select(
        IncrementalValuesProvider<InjectorInterfaceMetadata> metadataProvider
    ) {
        return metadataProvider.Select((metadata, _) =>
            InjectorMapper.Instance.Map(metadata, constructedSpecifications: []));
    }
}
