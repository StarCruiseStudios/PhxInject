// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyImplementationDefinition.cs" company="Star Cruise Studios LLC">
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

    internal delegate ExternalDependencyImplementationDefinition CreateExternalDependencyImplementationDefinition(
        ExternalDependencyDescriptor externalDependencyDescriptor,
        DefinitionGenerationContext context
    );

    internal record ExternalDependencyImplementationDefinition(
        TypeModel ExternalDependencyImplementationType,
        TypeModel ExternalDependencyInterfaceType,
        IEnumerable<ExternalDependencyProviderMethodDefinition> ProviderMethodDefinitions,
        Location Location
    ) : IDefinition {
        public class Builder {
            public ExternalDependencyImplementationDefinition Build(
                ExternalDependencyDescriptor externalDependencyDescriptor,
                DefinitionGenerationContext context
            ) {
                var implementationType = TypeHelpers.CreateExternalDependencyImplementationType(
                    context.Injector.InjectorType,
                    externalDependencyDescriptor.ExternalDependencyInterfaceType);

                var providers = externalDependencyDescriptor.Providers.Select(
                        provider => {
                            var specContainerFactoryInvocation = context.GetSpecContainerFactoryInvocation(
                                provider.ProvidedType,
                                provider.Location);

                            return new ExternalDependencyProviderMethodDefinition(
                                provider.ProvidedType.TypeModel,
                                provider.ProviderMethodName,
                                specContainerFactoryInvocation,
                                provider.Location);
                        })
                    .ToImmutableList();

                return new ExternalDependencyImplementationDefinition(
                    implementationType,
                    externalDependencyDescriptor.ExternalDependencyInterfaceType,
                    providers,
                    externalDependencyDescriptor.Location);
            }
        }
    }
}
