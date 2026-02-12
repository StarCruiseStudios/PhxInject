// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderMapper.cs" company="Star Cruise Studios LLC">
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
///     Mapper for transforming injector provider metadata into provider models.
/// </summary>
internal static class InjectorProviderMapper {
    /// <summary> Gets the singleton mapper instance. </summary>
    public static readonly InjectorProviderMapperInstance Instance = new();

    /// <summary>
    ///     Maps injector provider metadata to a provider model.
    /// </summary>
    /// <param name="metadata"> The provider metadata to map. </param>
    /// <returns> The mapped provider model. </returns>
    public static InjectorProviderModel Map(InjectorProviderMetadata metadata) {
        return Instance.Map(metadata);
    }
}

/// <summary>
///     Instance mapper for transforming injector provider metadata into provider models.
/// </summary>
internal class InjectorProviderMapperInstance {
    /// <summary>
    ///     Maps injector provider metadata to a provider model.
    /// </summary>
    /// <param name="metadata"> The provider metadata to map. </param>
    /// <returns> The mapped provider model. </returns>
    public InjectorProviderModel Map(InjectorProviderMetadata metadata) {
        return new InjectorProviderModel(
            metadata.ProvidedType,
            metadata.ProviderMethodName,
            metadata.Location
        );
    }
}
