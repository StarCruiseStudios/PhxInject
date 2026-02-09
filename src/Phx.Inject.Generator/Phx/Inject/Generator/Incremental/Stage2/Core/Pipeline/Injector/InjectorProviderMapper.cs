// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Injector;

internal static class InjectorProviderMapper {
    public static readonly InjectorProviderMapperInstance Instance = new();

    public static InjectorProviderModel Map(InjectorProviderMetadata metadata) {
        return Instance.Map(metadata);
    }
}

internal class InjectorProviderMapperInstance {
    public InjectorProviderModel Map(InjectorProviderMetadata metadata) {
        return new InjectorProviderModel(
            metadata.ProvidedType,
            metadata.ProviderMethodName,
            metadata.Location
        );
    }
}
