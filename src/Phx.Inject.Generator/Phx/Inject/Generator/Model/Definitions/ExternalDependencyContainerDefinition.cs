// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyContainerDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Model.Descriptors;

    delegate ExternalDependencyContainerDefinition CreateExternalDependencyContainerDefinition(
            ExternalDependencyDescriptor externalDependencyDescriptor,
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations);

    internal record ExternalDependencyContainerDefinition(
            TypeModel ExternalDependencyInterfaceType,
            IEnumerable<ExternalDependencyProviderMethodDefinition> ProviderMethodDefinitions,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateExternalDependencyProviderMethodDefinition
                    createExternalDependencyProviderMethodDefinition;

            public Builder(CreateExternalDependencyProviderMethodDefinition externalDependencyProviderMethodDefinition) {
                createExternalDependencyProviderMethodDefinition = externalDependencyProviderMethodDefinition;
            }

            public ExternalDependencyContainerDefinition Build(
                    ExternalDependencyDescriptor externalDependencyDescriptor,
                    IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations) {
                var providerDefinitions = externalDependencyDescriptor.Providers.Select(
                                provider => createExternalDependencyProviderMethodDefinition(
                                        externalDependencyDescriptor,
                                        provider,
                                        factoryRegistrations))
                        .ToImmutableList();

                return new ExternalDependencyContainerDefinition(
                        externalDependencyDescriptor.ExternalDependencyInterfaceType,
                        providerDefinitions,
                        externalDependencyDescriptor.Location);
            }
        }
    }
}
