// -----------------------------------------------------------------------------
// <copyright file="InjectorChildFactoryMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Model.Injector;

namespace Phx.Inject.Generator.Incremental.Stage2.Pipeline.Injector;

internal static class InjectorChildFactoryMapper {
    public static readonly InjectorChildFactoryMapperInstance Instance = new();

    public static InjectorChildFactoryModel Map(InjectorChildProviderMetadata metadata) {
        return Instance.Map(metadata);
    }
}

internal class InjectorChildFactoryMapperInstance {
    public InjectorChildFactoryModel Map(InjectorChildProviderMetadata metadata) {
        return new InjectorChildFactoryModel(
            metadata.ChildInjectorType,
            metadata.ChildProviderMethodName,
            metadata.Parameters,
            metadata.Location
        );
    }
}
