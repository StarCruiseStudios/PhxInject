// -----------------------------------------------------------------------------
//  <copyright file="InjectorManager.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Controller {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Phx.Inject.Generator.Model.Definitions;
    using Phx.Inject.Generator.Model.Descriptors;

    internal class InjectorManager {
        private readonly CreateInjectorProviderMethodDefinition createInjectorProviderMethod;
        private readonly CreateInjectorBuilderMethodDefinition createInjectorBuilderMethod;
        private readonly CreateSpecContainerCollectionDefinition createSpecContainerCollection;

        public InjectorManager(
                CreateInjectorProviderMethodDefinition createInjectorProviderMethod,
                CreateInjectorBuilderMethodDefinition createInjectorBuilderMethod,
                CreateSpecContainerCollectionDefinition createSpecContainerCollection
        ) {
            this.createInjectorProviderMethod = createInjectorProviderMethod;
            this.createInjectorBuilderMethod = createInjectorBuilderMethod;
            this.createSpecContainerCollection = createSpecContainerCollection;
        }

        public InjectorDefinition CreateInjectorDefinition(
                InjectorDescriptor injectorDescriptor,
                IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations,
                IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
        ) {
            var providerMethods = injectorDescriptor.Providers.Select(
                            provider => createInjectorProviderMethod(
                                    provider,
                                    injectorDescriptor,
                                    factoryRegistrations))
                    .ToImmutableList();
            var builderMethods = injectorDescriptor.Builders.Select(
                            builder => createInjectorBuilderMethod(builder, injectorDescriptor, builderRegistrations))
                    .ToImmutableList();

            var specContainerCollection = createSpecContainerCollection(
                    injectorDescriptor,
                    factoryRegistrations);

            return new InjectorDefinition(
                    InjectorType: injectorDescriptor.InjectorType,
                    InjectorInterfaceType: injectorDescriptor.InjectorInterface,
                    specContainerCollection,
                    providerMethods,
                    builderMethods,
                    injectorDescriptor.Location);
        }
    }
}
