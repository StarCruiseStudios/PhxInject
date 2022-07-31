// -----------------------------------------------------------------------------
//  <copyright file="InjectionContextDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using System.Collections.Immutable;
    using System.Linq;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate InjectionContextDefinition
            CreateInjectionContextDefinition(InjectorDescriptor injectorDescriptor);

    internal record InjectionContextDefinition(
            InjectorDefinition Injector,
            IEnumerable<SpecContainerDefinition> SpecContainers,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateInjectorDefinition createInjector;
            private readonly CreateSpecContainerDefinition createSpecContainer;

            public Builder(
                    CreateInjectorDefinition createInjector,
                    CreateSpecContainerDefinition createSpecContainer
            ) {
                this.createInjector = createInjector;
                this.createSpecContainer = createSpecContainer;
            }

            public InjectionContextDefinition Build(InjectorDescriptor injectorDescriptor) {
                IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
                        = new Dictionary<RegistrationIdentifier, FactoryRegistration>();
                IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
                        = new Dictionary<RegistrationIdentifier, BuilderRegistration>();

                // Create a registration for all of the spec descriptors' factory and builder methods.
                foreach (var specDescriptor in injectorDescriptor.Specifications) {
                    foreach (var factory in specDescriptor.Factories) {
                        factoryRegistrations.Add(
                                RegistrationIdentifier.FromQualifiedTypeDescriptor(factory.ReturnType),
                                new FactoryRegistration(specDescriptor, factory));
                    }

                    foreach (var builder in specDescriptor.Builders) {
                        builderRegistrations.Add(
                                RegistrationIdentifier.FromQualifiedTypeDescriptor(builder.BuiltType),
                                new BuilderRegistration(specDescriptor, builder));
                    }
                }

                // Create a registration for all of the spec descriptors' links. This must be done after all factory methods
                // have been registered to ensure that the link is valid.
                foreach (var specDescriptor in injectorDescriptor.Specifications) {
                    foreach (var link in specDescriptor.Links) {
                        if (factoryRegistrations.TryGetValue(
                                    RegistrationIdentifier.FromQualifiedTypeDescriptor(link.InputType),
                                    out var targetRegistration)) {
                            factoryRegistrations.Add(
                                    RegistrationIdentifier.FromQualifiedTypeDescriptor(link.ReturnType),
                                    targetRegistration);
                        } else {
                            throw new InjectionException(
                                    Diagnostics.IncompleteSpecification,
                                    $"Cannot find factory for type {link.InputType} required by link in specification {specDescriptor.SpecType}.",
                                    link.Location);
                        }
                    }
                }

                var injectorDefinition = createInjector(
                        injectorDescriptor,
                        factoryRegistrations,
                        builderRegistrations);

                var specContainerDefinitions = injectorDescriptor.Specifications.Select(
                                specDescriptor => createSpecContainer(
                                        specDescriptor,
                                        injectorDescriptor,
                                        factoryRegistrations))
                        .ToImmutableList();

                return new InjectionContextDefinition(
                        injectorDefinition,
                        specContainerDefinitions,
                        injectorDescriptor.Location);
            }
        }
    }
}
