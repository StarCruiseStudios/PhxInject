// -----------------------------------------------------------------------------
//  <copyright file="InjectorDefinition.cs" company="Star Cruise Studios LLC">
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

    internal delegate InjectorDefinition CreateInjectorDefinition(
            InjectorDescriptor injectorDescriptor,
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations,
            IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
    );

    internal record InjectorDefinition(
            TypeModel InjectorType,
            TypeModel InjectorInterfaceType,
            SpecContainerCollectionDefinition SpecContainerCollection,
            IEnumerable<ExternalDependencyContainerDefinition> externalDependencies,
            IEnumerable<InjectorProviderMethodDefinition> ProviderMethods,
            IEnumerable<InjectorBuilderMethodDefinition> BuilderMethods,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateExternalDependencyContainerDefinition createExternalDependency;
            private readonly CreateInjectorProviderMethodDefinition createInjectorProviderMethod;
            private readonly CreateInjectorBuilderMethodDefinition createInjectorBuilderMethod;
            private readonly CreateSpecContainerCollectionDefinition createSpecContainerCollection;

            public Builder(
                    CreateExternalDependencyContainerDefinition createExternalDependency,
                    CreateInjectorProviderMethodDefinition createInjectorProviderMethod,
                    CreateInjectorBuilderMethodDefinition createInjectorBuilderMethod,
                    CreateSpecContainerCollectionDefinition createSpecContainerCollection
            ) {
                this.createExternalDependency = createExternalDependency;
                this.createInjectorProviderMethod = createInjectorProviderMethod;
                this.createInjectorBuilderMethod = createInjectorBuilderMethod;
                this.createSpecContainerCollection = createSpecContainerCollection;
            }

            public InjectorDefinition Build(
                    InjectorDescriptor injectorDescriptor,
                    IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations,
                    IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
            ) {
                var externalDependencies = injectorDescriptor.ExternalDependencies.Select(
                                externalDependency => createExternalDependency(
                                        externalDependency,
                                        factoryRegistrations))
                        .ToImmutableList();
                var providerMethods = injectorDescriptor.Providers.Select(
                                provider => createInjectorProviderMethod(
                                        provider,
                                        injectorDescriptor,
                                        factoryRegistrations))
                        .ToImmutableList();
                var builderMethods = injectorDescriptor.Builders.Select(
                                builder => createInjectorBuilderMethod(
                                        builder,
                                        injectorDescriptor,
                                        builderRegistrations))
                        .ToImmutableList();

                var specContainerCollection = createSpecContainerCollection(injectorDescriptor);

                return new InjectorDefinition(
                        InjectorType: injectorDescriptor.InjectorType,
                        InjectorInterfaceType: injectorDescriptor.InjectorInterfaceType,
                        specContainerCollection,
                        externalDependencies,
                        providerMethods,
                        builderMethods,
                        injectorDescriptor.Location);
            }
        }
    }
}
