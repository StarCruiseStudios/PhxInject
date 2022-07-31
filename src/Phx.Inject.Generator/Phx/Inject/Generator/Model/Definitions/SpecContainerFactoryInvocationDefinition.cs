// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryInvocationDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate SpecContainerFactoryInvocationDefinition CreateSpecContainerFactoryInvocationDefinition(
            InjectorDescriptor injectorDescriptor,
            FactoryRegistration factoryRegistration,
            Location location);

    internal record SpecContainerFactoryInvocationDefinition(
            SpecContainerReferenceDefinition ContainerReference,
            string FactoryMethodName,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateSpecContainerReferenceDefinition createSpecContainerReference;

            public Builder(CreateSpecContainerReferenceDefinition createSpecContainerReference) {
                this.createSpecContainerReference = createSpecContainerReference;
            }

            public SpecContainerFactoryInvocationDefinition Build(
                    InjectorDescriptor injectorDescriptor,
                    FactoryRegistration factoryRegistration,
                    Location location
            ) {
                return new SpecContainerFactoryInvocationDefinition(
                        createSpecContainerReference(injectorDescriptor, factoryRegistration.Specification),
                        factoryRegistration.FactoryDescriptor.FactoryMethodName,
                        location);
            }
        }
    }
}
