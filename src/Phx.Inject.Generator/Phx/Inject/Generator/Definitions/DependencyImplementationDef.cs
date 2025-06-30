// -----------------------------------------------------------------------------
//  <copyright file="DependencyImplementationDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Definitions {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Descriptors;
    using Phx.Inject.Generator.Model;

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

                var providers = dependencyDesc.Providers.Select(
                        provider => {
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
}
