// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Definitions {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Model.Specifications.Descriptors;

    internal delegate SpecContainerDefinition CreateSpecContainerDefinition(
            SpecDescriptor specDescriptor,
            IDefinitionGenerationContext context
    );

    internal record SpecContainerDefinition(
            TypeModel SpecContainerType,
            TypeModel SpecificationType,
            SpecInstantiationMode SpecInstantiationMode,
            IEnumerable<SpecContainerFactoryDefinition> FactoryMethodDefinitions,
            IEnumerable<SpecContainerBuilderDefinition> BuilderMethodDefinitions,
            Location Location
    ) : IDefinition {
        public class Builder {
            public SpecContainerDefinition Build(SpecDescriptor specDescriptor, IDefinitionGenerationContext context) {
                var specContainerType = SymbolProcessors.CreateSpecContainerType(
                        context.InjectorType,
                        specDescriptor.SpecType);

                var factories = specDescriptor.Factories.Select(
                        factory => {
                            var arguments = factory.Parameters.Select(
                                            parameter => {
                                                if (!context.FactoryRegistrations.TryGetValue(
                                                            RegistrationIdentifier.FromQualifiedTypeDescriptor(
                                                                    parameter),
                                                            out var factoryRegistration)) {
                                                    throw new InjectionException(
                                                            Diagnostics.IncompleteSpecification,
                                                            $"Cannot find factory for type {parameter} required by factory method "
                                                            + $"{factory.FactoryMethodName} in specification {specDescriptor.SpecType} "
                                                            + $"in injector type {context.InjectorType}.",
                                                            factory.Location);
                                                }

                                                return new SpecContainerFactoryInvocationDefinition(
                                                        factoryRegistration.Specification,
                                                        factoryRegistration.FactoryDescriptor.FactoryMethodName,
                                                        factoryRegistration.FactoryDescriptor.Location);
                                            })
                                    .ToImmutableList();

                            return new SpecContainerFactoryDefinition(
                                    factory.ReturnType.TypeModel,
                                    factory.FactoryMethodName,
                                    context.SpecContainerCollectionType,
                                    factory.FabricationMode,
                                    arguments,
                                    factory.Location);
                        });

                var builders = specDescriptor.Builders.Select(
                        builder => {
                            var arguments = builder.Parameters.Select(
                                            parameter => {
                                                if (!context.FactoryRegistrations.TryGetValue(
                                                            RegistrationIdentifier.FromQualifiedTypeDescriptor(
                                                                    parameter),
                                                            out var factoryRegistration)) {
                                                    throw new InjectionException(
                                                            Diagnostics.IncompleteSpecification,
                                                            $"Cannot find factory for type {parameter} required by builder method "
                                                            + $"{builder.BuilderMethodName} in specification {specDescriptor.SpecType} "
                                                            + $"in injector type {context.InjectorType}.",
                                                            builder.Location);
                                                }

                                                return new SpecContainerFactoryInvocationDefinition(
                                                        factoryRegistration.Specification,
                                                        factoryRegistration.FactoryDescriptor.FactoryMethodName,
                                                        factoryRegistration.FactoryDescriptor.Location);
                                            })
                                    .ToImmutableList();

                            return new SpecContainerBuilderDefinition(
                                    builder.BuiltType.TypeModel,
                                    builder.BuilderMethodName,
                                    context.SpecContainerCollectionType,
                                    arguments,
                                    builder.Location);
                        });

                return new SpecContainerDefinition(
                        specContainerType,
                        specDescriptor.SpecType,
                        specDescriptor.InstantiationMode,
                        factories,
                        builders,
                        specDescriptor.Location);
            }
        }
    }
}
