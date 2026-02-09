// -----------------------------------------------------------------------------
// <copyright file="InjectorBuilderMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Injector;

internal static class InjectorBuilderMapper {
    public static readonly InjectorBuilderMapperInstance Instance = new();

    public static InjectorBuilderModel Map(InjectorActivatorMetadata metadata) {
        return Instance.Map(metadata);
    }
}

internal class InjectorBuilderMapperInstance {
    public InjectorBuilderModel Map(InjectorActivatorMetadata metadata) {
        return new InjectorBuilderModel(
            metadata.ActivatedType,
            metadata.ActivatorMethodName,
            metadata.Location
        );
    }
}
