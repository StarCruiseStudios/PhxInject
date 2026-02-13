// -----------------------------------------------------------------------------
// <copyright file="InjectorChildFactoryMapper.cs" company="Star Cruise Studios LLC">
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
///     Mapper for transforming injector child provider metadata into child factory models.
/// </summary>
internal static class InjectorChildFactoryMapper {
    /// <summary> Gets the singleton mapper instance. </summary>
    public static readonly InjectorChildFactoryMapperInstance Instance = new();

    /// <summary>
    ///     Maps injector child provider metadata to a child factory model.
    /// </summary>
    /// <param name="metadata"> The child provider metadata to map. </param>
    /// <returns> The mapped child factory model. </returns>
    public static InjectorChildFactoryModel Map(InjectorChildProviderMetadata metadata) {
        return Instance.Map(metadata);
    }
}

/// <summary>
///     Instance mapper for transforming injector child provider metadata into child factory models.
/// </summary>
internal class InjectorChildFactoryMapperInstance {
    /// <summary>
    ///     Maps injector child provider metadata to a child factory model.
    /// </summary>
    /// <param name="metadata"> The child provider metadata to map. </param>
    /// <returns> The mapped child factory model. </returns>
    public InjectorChildFactoryModel Map(InjectorChildProviderMetadata metadata) {
        return new InjectorChildFactoryModel(
            metadata.ChildInjectorType,
            metadata.ChildProviderMethodName,
            metadata.Parameters,
            metadata.Location
        );
    }
}
