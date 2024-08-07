﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectionContextDefinition.cs" company="Star Cruise Studios LLC">
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

    internal delegate InjectionContextDefinition CreateInjectionContextDefinition(
        InjectorDescriptor injectorDescriptor,
        DefinitionGenerationContext context
    );

    internal record InjectionContextDefinition(
        InjectorDefinition Injector,
        IEnumerable<SpecContainerDefinition> SpecContainers,
        IEnumerable<ExternalDependencyImplementationDefinition> ExternalDependencyImplementations,
        Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateExternalDependencyImplementationDefinition createExternalDependencyImplementation;
            private readonly CreateInjectorDefinition createInjector;
            private readonly CreateSpecContainerDefinition createSpecContainer;

            public Builder(
                CreateInjectorDefinition createInjector,
                CreateSpecContainerDefinition createSpecContainer,
                CreateExternalDependencyImplementationDefinition createExternalDependencyImplementation
            ) {
                this.createInjector = createInjector;
                this.createSpecContainer = createSpecContainer;
                this.createExternalDependencyImplementation = createExternalDependencyImplementation;
            }

            public InjectionContextDefinition Build(
                InjectorDescriptor injectorDescriptor,
                DefinitionGenerationContext context
            ) {
                var factoryRegistrations = new Dictionary<RegistrationIdentifier, List<FactoryRegistration>>();
                var builderRegistrations = new Dictionary<RegistrationIdentifier, BuilderRegistration>();

                var specDescriptors = context.Specifications.Values.ToImmutableList();

                // Create a registration for all of the spec descriptors' factory and builder methods.
                foreach (var specDescriptor in specDescriptors) {
                    foreach (var factory in specDescriptor.Factories) {
                        List<FactoryRegistration> registrationList;
                        var key = RegistrationIdentifier.FromQualifiedTypeDescriptor(factory.ReturnType);
                        if (factoryRegistrations.TryGetValue(key, out registrationList)) {
                            if (!registrationList.First().FactoryDescriptor.isPartial || !factory.isPartial) {
                                throw new InjectionException(
                                    Diagnostics.InvalidSpecification,
                                    $"Factory for type {factory.ReturnType} must be unique or all factories must be partial.",
                                    factory.Location);
                            }
                        } else {
                            registrationList = new List<FactoryRegistration>();
                            factoryRegistrations.Add(key, registrationList);
                        }

                        registrationList.Add(new FactoryRegistration(specDescriptor, factory));
                    }

                    foreach (var builder in specDescriptor.Builders) {
                        builderRegistrations.Add(
                            RegistrationIdentifier.FromQualifiedTypeDescriptor(builder.BuiltType),
                            new BuilderRegistration(specDescriptor, builder));
                    }
                }

                // Create a registration for all of the spec descriptors' links. This must be done after all factory methods
                // have been registered to ensure that the link is valid.
                foreach (var specDescriptor in specDescriptors) {
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

                var generationContext = context with {
                    FactoryRegistrations = factoryRegistrations,
                    BuilderRegistrations = builderRegistrations
                };

                var injectorDefinition = createInjector(generationContext);

                var specContainerDefinitions = specDescriptors.Select(
                        specDescriptor => createSpecContainer(specDescriptor, generationContext))
                    .ToImmutableList();

                var externalDependencyImplementationDefinitions = injectorDescriptor.ChildFactories
                    .Select(
                        childFactory => generationContext.GetInjector(
                            childFactory.ChildInjectorType,
                            childFactory.Location))
                    .SelectMany(childInjector => childInjector.ExternalDependencyInterfaceTypes)
                    .GroupBy(externalDependencyType => externalDependencyType)
                    .Select(externalDependencyTypeGroup => externalDependencyTypeGroup.First())
                    .Select(
                        externalDependencyType => generationContext.GetExternalDependency(
                            externalDependencyType,
                            injectorDescriptor.Location))
                    .Select(
                        externalDependency => createExternalDependencyImplementation(
                            externalDependency,
                            generationContext))
                    .ToImmutableList();

                return new InjectionContextDefinition(
                    injectorDefinition,
                    specContainerDefinitions,
                    externalDependencyImplementationDefinitions,
                    injectorDescriptor.Location);
            }
        }
    }
}
