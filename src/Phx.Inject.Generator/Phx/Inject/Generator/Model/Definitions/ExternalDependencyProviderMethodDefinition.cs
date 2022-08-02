// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyProviderMethodDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate ExternalDependencyProviderMethodDefinition CreateExternalDependencyProviderMethodDefinition(
            ExternalDependencyDescriptor externalDependencyDescriptor,
            ExternalDependencyProviderDescriptor providerDescriptor,
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations);

    internal record ExternalDependencyProviderMethodDefinition(
            TypeModel ProvidedType,
            string ProviderMethodName,
            ExternalDependencySpecFactoryInvocationDefinition SpecContainerFactoryInvocation,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateExternalDependencySpecFactoryInvocationDefinition createExternalDependencySpecFactoryInvocation;

            public Builder(
                    CreateExternalDependencySpecFactoryInvocationDefinition createExternalDependencySpecFactoryInvocation
            ) {
                this.createExternalDependencySpecFactoryInvocation = createExternalDependencySpecFactoryInvocation;
            }

            public ExternalDependencyProviderMethodDefinition Build(
                    ExternalDependencyDescriptor externalDependencyDescriptor,
                    ExternalDependencyProviderDescriptor providerDescriptor,
                    IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
            ) {
                if (!factoryRegistrations.TryGetValue(
                            RegistrationIdentifier.FromQualifiedTypeDescriptor(providerDescriptor.ProvidedType),
                            out var factoryRegistration)) {
                    throw new InjectionException(
                            Diagnostics.IncompleteSpecification,
                            $"Cannot find factory for type {providerDescriptor.ProvidedType} required by external dependency {externalDependencyDescriptor.ExternalDependencyInterfaceType}",
                            providerDescriptor.Location);
                }

                var specContainerFactoryInvocation = createExternalDependencySpecFactoryInvocation(
                        factoryRegistration,
                        providerDescriptor.Location);

                return new ExternalDependencyProviderMethodDefinition(
                        providerDescriptor.ProvidedType.TypeModel,
                        providerDescriptor.ProviderMethodName,
                        specContainerFactoryInvocation,
                        providerDescriptor.Location);
            }
        }
    };
}
