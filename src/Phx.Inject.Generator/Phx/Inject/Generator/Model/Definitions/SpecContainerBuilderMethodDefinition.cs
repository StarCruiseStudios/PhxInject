// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderMethodDefinition.cs" company="Star Cruise Studios LLC">
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

    internal delegate SpecContainerBuilderMethodDefinition CreateSpecContainerBuilderMethodDefinition(
            InjectorDescriptor injectorDescriptor,
            SpecDescriptor specDescriptor,
            SpecBuilderDescriptor specBuilderDescriptor,
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
    );

    internal record SpecContainerBuilderMethodDefinition(
            TypeModel BuiltType,
            SpecReferenceDefinition SpecReference,
            string MethodName,
            TypeModel SpecContainerType,
            TypeModel SpecContainerCollectionType,
            IEnumerable<SpecContainerFactoryInvocationDefinition> Arguments,
            Location Location) : IDefinition {
        public class Builder {
            private readonly CreateSpecReferenceDefinition createSpecReference;
            private readonly CreateSpecContainerType createSpecContainerType;
            private readonly CreateSpecContainerCollectionType createSpecContainerCollectionType;
            private readonly CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation;

            public Builder(
                    CreateSpecReferenceDefinition createSpecReference,
                    CreateSpecContainerType createSpecContainerType,
                    CreateSpecContainerCollectionType createSpecContainerCollectionType,
                    CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation
            ) {
                this.createSpecReference = createSpecReference;
                this.createSpecContainerType = createSpecContainerType;
                this.createSpecContainerCollectionType = createSpecContainerCollectionType;
                this.createSpecContainerFactoryInvocation = createSpecContainerFactoryInvocation;
            }

            public SpecContainerBuilderMethodDefinition Build(
                    InjectorDescriptor injectorDescriptor,
                    SpecDescriptor specDescriptor,
                    SpecBuilderDescriptor specBuilderDescriptor,
                    IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
            ) {
                var specReference = createSpecReference(specDescriptor);
                var specContainerType = createSpecContainerType(
                        injectorDescriptor.InjectorType,
                        specDescriptor.SpecType);
                var specContainerCollectionType = createSpecContainerCollectionType(injectorDescriptor.InjectorType);

                var arguments = specBuilderDescriptor.Arguments.Select(
                        argumentType => {
                            if (!factoryRegistrations.TryGetValue(
                                        RegistrationIdentifier.FromQualifiedTypeDescriptor(argumentType),
                                        out var factoryRegistration)) {
                                throw new InjectionException(
                                        Diagnostics.IncompleteSpecification,
                                        $"Cannot find factory for type {argumentType} required by builder method "
                                        + $"{specBuilderDescriptor.BuilderMethodName} in specification {specDescriptor.SpecType} "
                                        + $"in injector type {injectorDescriptor.InjectorType}.",
                                        argumentType.Location);
                            }

                            return createSpecContainerFactoryInvocation(
                                    injectorDescriptor,
                                    factoryRegistration,
                                    argumentType.Location);
                        }).ToImmutableList();

                return new SpecContainerBuilderMethodDefinition(
                        specBuilderDescriptor.BuiltType.TypeModel,
                        specReference,
                        specBuilderDescriptor.BuilderMethodName,
                        specContainerType,
                        specContainerCollectionType,
                        arguments,
                        specBuilderDescriptor.Location
                );
            }
        }
    }
}
