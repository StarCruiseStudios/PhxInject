// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryMethodDefinition.cs" company="Star Cruise Studios LLC">
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

    internal delegate SpecContainerFactoryMethodDefinition CreateSpecContainerFactoryMethodDefinition(
            InjectorDescriptor injectorDescriptor,
            SpecDescriptor specDescriptor,
            SpecFactoryDescriptor specFactoryDescriptor,
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
    );

    internal record SpecContainerFactoryMethodDefinition(
            TypeModel ProvidedType,
            SpecReferenceDefinition SpecReference,
            string MethodName,
            TypeModel SpecContainerType,
            TypeModel SpecContainerCollectionType,
            SpecContainerFactoryInstanceHolderDefinition? InstanceHolder,
            IEnumerable<SpecContainerFactoryInvocationDefinition> Arguments,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateSpecReferenceDefinition createSpecReference;
            private readonly CreateSpecContainerType createSpecContainerType;
            private readonly CreateSpecContainerCollectionType createSpecContainerCollectionType;
            private readonly CreateSpecContainerFactoryInstanceHolderDefinition createInstanceHolder;
            private readonly CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation;

            public Builder(
                    CreateSpecReferenceDefinition createSpecReference,
                    CreateSpecContainerType createSpecContainerType,
                    CreateSpecContainerCollectionType createSpecContainerCollectionType,
                    CreateSpecContainerFactoryInstanceHolderDefinition createInstanceHolder,
                    CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation
            ) {
                this.createSpecReference = createSpecReference;
                this.createSpecContainerType = createSpecContainerType;
                this.createSpecContainerCollectionType = createSpecContainerCollectionType;
                this.createInstanceHolder = createInstanceHolder;
                this.createSpecContainerFactoryInvocation = createSpecContainerFactoryInvocation;
            }

            public SpecContainerFactoryMethodDefinition Build(
                    InjectorDescriptor injectorDescriptor,
                    SpecDescriptor specDescriptor,
                    SpecFactoryDescriptor specFactoryDescriptor,
                    IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
            ) {
                var specReference = createSpecReference(specDescriptor);
                var specContainerType = createSpecContainerType(
                        injectorDescriptor.InjectorType,
                        specDescriptor.SpecType);
                var specContainerCollectionType = createSpecContainerCollectionType(injectorDescriptor.InjectorType);
                var instanceHolder = createInstanceHolder(specFactoryDescriptor);

                var arguments = specFactoryDescriptor.Arguments.Select(
                        argumentType => {
                            if (!factoryRegistrations.TryGetValue(
                                        RegistrationIdentifier.FromQualifiedTypeDescriptor(argumentType),
                                        out var factoryRegistration)) {
                                throw new InjectionException(
                                        Diagnostics.IncompleteSpecification,
                                        $"Cannot find factory for type {argumentType} required by factory method "
                                        + $"{specFactoryDescriptor.FactoryMethodName} in specification {specDescriptor.SpecType} "
                                        + $"in injector type {injectorDescriptor.InjectorType}.",
                                        argumentType.Location);
                            }

                            return createSpecContainerFactoryInvocation(
                                    injectorDescriptor,
                                    factoryRegistration,
                                    argumentType.Location);
                        }).ToImmutableList();

                return new SpecContainerFactoryMethodDefinition(
                        specFactoryDescriptor.ReturnType.TypeModel,
                        specReference,
                        specFactoryDescriptor.FactoryMethodName,
                        specContainerType,
                        specContainerCollectionType,
                        instanceHolder,
                        arguments,
                        specFactoryDescriptor.Location
                );
            }
        }
    }
}
