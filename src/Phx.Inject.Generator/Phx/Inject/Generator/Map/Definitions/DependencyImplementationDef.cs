// -----------------------------------------------------------------------------
//  <copyright file="DependencyImplementationDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Map.Definitions;

internal record DependencyImplementationDef(
    TypeModel DependencyImplementationType,
    TypeModel DependencyInterfaceType,
    IEnumerable<DependencyProviderMethodDef> ProviderMethodDefs,
    Location Location
) : IDefinition {
    public interface IBuilder {
        DependencyImplementationDef Build(
            DependencyDesc dependencyDesc,
            DefGenerationContext context
        );
    }

    public class Builder : IBuilder {
        public DependencyImplementationDef Build(
            DependencyDesc dependencyDesc,
            DefGenerationContext context
        ) {
            var implementationType = TypeHelpers.CreateDependencyImplementationType(
                context.Injector.InjectorType,
                dependencyDesc.DependencyInterfaceType);

            IReadOnlyList<DependencyProviderMethodDef> providers = dependencyDesc.Providers.Select(provider => {
                    var specContainerFactoryInvocation = context.GetSpecContainerFactoryInvocation(
                        provider.ProvidedType,
                        provider.Location);

                    return new DependencyProviderMethodDef(
                        provider.ProvidedType.TypeModel,
                        provider.ProviderMethodName,
                        specContainerFactoryInvocation,
                        provider.Location);
                })
                .ToImmutableList();

            return new DependencyImplementationDef(
                implementationType,
                dependencyDesc.DependencyInterfaceType,
                providers,
                dependencyDesc.Location);
        }
    }
}
