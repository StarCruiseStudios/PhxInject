// -----------------------------------------------------------------------------
// <copyright file="InjectorModelMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Model.Injector;

namespace Phx.Inject.Generator.Incremental.Stage2.Mappers;

internal static class InjectorModelMapper {
    public static InjectorModel MapToModel(InjectorInterfaceMetadata metadata) {
        return new InjectorModel(
            InjectorInterfaceType: metadata.InjectorInterfaceType,
            Providers: metadata.Providers.Select(p => new ProviderModel(
                ProviderMethodName: p.ProviderMethodName,
                ProvidedType: p.ProvidedType
            )),
            Activators: metadata.Activators.Select(a => new ActivatorModel(
                ActivatorMethodName: a.ActivatorMethodName,
                ActivatedType: a.ActivatedType
            )),
            ChildProviders: metadata.ChildProviders.Select(c => new ChildProviderModel(
                ChildProviderMethodName: c.ChildProviderMethodName,
                ChildInjectorType: c.ChildInjectorType,
                Parameters: c.Parameters
            ))
        );
    }
}
