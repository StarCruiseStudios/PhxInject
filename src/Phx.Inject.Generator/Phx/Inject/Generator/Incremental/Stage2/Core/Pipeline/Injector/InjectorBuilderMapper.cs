// -----------------------------------------------------------------------------
// <copyright file="InjectorBuilderMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Injector;

/// <summary>
///     Mapper for transforming injector activator metadata into builder models.
/// </summary>
internal static class InjectorBuilderMapper {
    /// <summary> Gets the singleton mapper instance. </summary>
    public static readonly InjectorBuilderMapperInstance Instance = new();

    /// <summary>
    ///     Maps injector activator metadata to an injector builder model.
    /// </summary>
    /// <param name="metadata"> The activator metadata to map. </param>
    /// <returns> The mapped injector builder model. </returns>
    public static InjectorBuilderModel Map(InjectorActivatorMetadata metadata) {
        return Instance.Map(metadata);
    }
}

/// <summary>
///     Instance mapper for transforming injector activator metadata into builder models.
/// </summary>
internal class InjectorBuilderMapperInstance {
    /// <summary>
    ///     Maps injector activator metadata to an injector builder model.
    /// </summary>
    /// <param name="metadata"> The activator metadata to map. </param>
    /// <returns> The mapped injector builder model. </returns>
    public InjectorBuilderModel Map(InjectorActivatorMetadata metadata) {
        return new InjectorBuilderModel(
            metadata.ActivatedType,
            metadata.ActivatorMethodName,
            metadata.Location
        );
    }
}
