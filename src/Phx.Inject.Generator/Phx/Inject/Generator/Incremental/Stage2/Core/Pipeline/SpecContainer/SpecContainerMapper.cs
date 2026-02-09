// -----------------------------------------------------------------------------
// <copyright file="SpecContainerMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.SpecContainer;

internal static class SpecContainerMapper {
    public static readonly SpecContainerMapperInstance Instance = new();

    public static SpecContainerModel Map(TypeMetadata injectorType, SpecClassMetadata metadata) {
        return Instance.Map(injectorType, metadata);
    }

    public static SpecContainerModel Map(TypeMetadata injectorType, SpecInterfaceMetadata metadata) {
        return Instance.Map(injectorType, metadata);
    }

    public static SpecContainerModel Map(
        TypeMetadata injectorType,
        InjectorDependencyInterfaceMetadata metadata
    ) {
        return Instance.Map(injectorType, metadata);
    }
}

internal class SpecContainerMapperInstance {
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
