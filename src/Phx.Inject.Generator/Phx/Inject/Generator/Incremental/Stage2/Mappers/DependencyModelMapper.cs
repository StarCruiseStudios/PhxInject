// -----------------------------------------------------------------------------
// <copyright file="DependencyModelMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage2.Model.Injector;

namespace Phx.Inject.Generator.Incremental.Stage2.Mappers;

internal static class DependencyModelMapper {
    public static DependencyModel MapToModel(InjectorDependencyInterfaceMetadata metadata) {
        return new DependencyModel(
            InjectorDependencyInterfaceType: metadata.InjectorDependencyInterfaceType,
            FactoryMethods: metadata.FactoryMethods.Select(f => new DependencyFactoryMethodModel(
                FactoryMethodName: f.FactoryMethodName,
                FactoryReturnType: f.FactoryReturnType,
                Parameters: f.Parameters
            )),
            FactoryProperties: metadata.FactoryProperties.Select(f => new DependencyFactoryPropertyModel(
                FactoryPropertyName: f.FactoryPropertyName,
                FactoryReturnType: f.FactoryReturnType
            ))
        );
    }
}
