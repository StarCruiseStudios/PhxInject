// -----------------------------------------------------------------------------
// <copyright file="SpecContainerMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.SpecContainer;

/// <summary>
///     Mapper for transforming specification metadata into spec container models.
/// </summary>
internal static class SpecContainerMapper {
    /// <summary> Gets the singleton mapper instance. </summary>
    public static readonly SpecContainerMapperInstance Instance = new();

    /// <summary>
    ///     Maps specification class metadata to a spec container model.
    /// </summary>
    /// <param name="injectorType"> The injector type that owns this spec container. </param>
    /// <param name="metadata"> The specification class metadata to map. </param>
    /// <returns> The mapped spec container model. </returns>
    public static SpecContainerModel Map(TypeMetadata injectorType, SpecClassMetadata metadata) {
        return Instance.Map(injectorType, metadata);
    }

    /// <summary>
    ///     Maps specification interface metadata to a spec container model.
    /// </summary>
    /// <param name="injectorType"> The injector type that owns this spec container. </param>
    /// <param name="metadata"> The specification interface metadata to map. </param>
    /// <returns> The mapped spec container model. </returns>
    public static SpecContainerModel Map(TypeMetadata injectorType, SpecInterfaceMetadata metadata) {
        return Instance.Map(injectorType, metadata);
    }

    /// <summary>
    ///     Maps injector dependency interface metadata to a spec container model.
    /// </summary>
    /// <param name="injectorType"> The injector type that owns this spec container. </param>
    /// <param name="metadata"> The dependency interface metadata to map. </param>
    /// <returns> The mapped spec container model. </returns>
    public static SpecContainerModel Map(
        TypeMetadata injectorType,
        InjectorDependencyInterfaceMetadata metadata
    ) {
        return Instance.Map(injectorType, metadata);
    }
}

/// <summary>
///     Instance mapper for transforming specification metadata into spec container models.
/// </summary>
internal sealed class SpecContainerMapperInstance {
    /// <summary>
    ///     Maps specification class metadata to a spec container model.
    /// </summary>
    /// <param name="injectorType"> The injector type that owns this spec container. </param>
    /// <param name="metadata"> The specification class metadata to map. </param>
    /// <returns> The mapped spec container model. </returns>
    public SpecContainerModel Map(TypeMetadata injectorType, SpecClassMetadata metadata) {
        var specType = metadata.SpecType;
        var specContainerType = injectorType.CreateSpecContainerType(specType);
        var factories = metadata.FactoryMethods.Select(SpecContainerFactoryMapper.Map)
            .Concat(metadata.FactoryProperties.Select(SpecContainerFactoryMapper.Map))
            .Concat(metadata.FactoryReferences.Select(SpecContainerFactoryMapper.Map));
        var builders = metadata.BuilderMethods.Select(SpecContainerBuilderMapper.Map)
            .Concat(metadata.BuilderReferences.Select(SpecContainerBuilderMapper.Map));

        return new SpecContainerModel(
            specContainerType,
            specType,
            SpecInstantiationMode.Static,
            factories,
            builders,
            metadata.Location
        );
    }

    /// <summary>
    ///     Maps specification interface metadata to a spec container model.
    /// </summary>
    /// <param name="injectorType"> The injector type that owns this spec container. </param>
    /// <param name="metadata"> The specification interface metadata to map. </param>
    /// <returns> The mapped spec container model. </returns>
    public SpecContainerModel Map(TypeMetadata injectorType, SpecInterfaceMetadata metadata) {
        var specType = metadata.SpecInterfaceType;
        var specContainerType = injectorType.CreateSpecContainerType(specType);
        var factories = metadata.FactoryMethods.Select(SpecContainerFactoryMapper.Map)
            .Concat(metadata.FactoryProperties.Select(SpecContainerFactoryMapper.Map))
            .Concat(metadata.FactoryReferences.Select(SpecContainerFactoryMapper.Map));
        var builders = metadata.BuilderMethods.Select(SpecContainerBuilderMapper.Map)
            .Concat(metadata.BuilderReferences.Select(SpecContainerBuilderMapper.Map));

        return new SpecContainerModel(
            specContainerType,
            specType,
            SpecInstantiationMode.Instantiated,
            factories,
            builders,
            metadata.Location
        );
    }

    /// <summary>
    ///     Maps injector dependency interface metadata to a spec container model.
    /// </summary>
    /// <param name="injectorType"> The injector type that owns this spec container. </param>
    /// <param name="metadata"> The dependency interface metadata to map. </param>
    /// <returns> The mapped spec container model. </returns>
    public SpecContainerModel Map(
        TypeMetadata injectorType,
        InjectorDependencyInterfaceMetadata metadata
    ) {
        var specType = metadata.InjectorDependencyInterfaceType;
        var specContainerType = injectorType.CreateSpecContainerType(specType);
        var factories = metadata.FactoryMethods.Select(SpecContainerFactoryMapper.Map)
            .Concat(metadata.FactoryProperties.Select(SpecContainerFactoryMapper.Map));

        return new SpecContainerModel(
            specContainerType,
            specType,
            SpecInstantiationMode.Dependency,
            factories,
            [],
            metadata.Location
        );
    }
}
