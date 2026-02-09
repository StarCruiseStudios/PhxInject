// -----------------------------------------------------------------------------
// <copyright file="InjectorMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage2.Model.Injector;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage2.Pipeline.Injector;

internal static class InjectorMapper {
    public static readonly InjectorMapperInstance Instance = new();

    public static InjectorModel Map(
        InjectorInterfaceMetadata metadata,
        IEnumerable<TypeMetadata>? constructedSpecifications = null
    ) {
        return Instance.Map(metadata, constructedSpecifications ?? []);
    }
}

internal class InjectorMapperInstance {
    public InjectorModel Map(
        InjectorInterfaceMetadata metadata,
        IEnumerable<TypeMetadata> constructedSpecifications
    ) {
        var attr = metadata.InjectorAttributeMetadata;
        var injectorType = metadata.InjectorInterfaceType.CreateInjectorType(attr.GeneratedClassName);
        var constructedSet = new HashSet<TypeMetadata>(constructedSpecifications);
        var constructedList = metadata.InjectorAttributeMetadata.Specifications
            .Where(s => constructedSet.Contains(s))
            .ToList();

        return new InjectorModel(
            injectorType,
            metadata.InjectorInterfaceType,
            attr.Specifications,
            constructedList,
            metadata.DependencyAttributeMetadata?.DependencyType,
            metadata.Providers.Select(InjectorProviderMapper.Map),
            metadata.Activators.Select(InjectorBuilderMapper.Map),
            metadata.ChildProviders.Select(InjectorChildFactoryMapper.Map),
            metadata.Location
        );
    }
}
