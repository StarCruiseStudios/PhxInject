// -----------------------------------------------------------------------------
// <copyright file="InjectorMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Injector;

internal class InjectorMapper {
    public static readonly InjectorMapper Instance = new();

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