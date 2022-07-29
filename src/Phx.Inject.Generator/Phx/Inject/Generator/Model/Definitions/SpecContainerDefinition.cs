// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate SpecContainerDefinition CreateSpecContainerDefinition(
            SpecDescriptor specDescriptor,
            InjectorDescriptor injectorDescriptor,
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
    );

    internal record SpecContainerDefinition(
            TypeModel ContainerType,
            SpecReferenceDefinition SpecReference,
            IEnumerable<SpecContainerFactoryInstanceHolderDefinition> InstanceHolderDeclarations,
            IEnumerable<SpecContainerFactoryMethodDefinition> FactoryMethodDefinitions,
            IEnumerable<SpecContainerBuilderMethodDefinition> BuilderMethodDefinitions,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateSpecContainerType createSpecContainerType;
            private readonly CreateSpecReferenceDefinition createSpecReference;

            private readonly CreateSpecContainerFactoryInstanceHolderDefinition createSpecContainerFactoryInstanceHolder;
            private readonly CreateSpecContainerFactoryMethodDefinition createSpecContainerFactoryMethod;
            private readonly CreateSpecContainerBuilderMethodDefinition createSpecContainerBuilderMethod;

            public Builder(
                    CreateSpecContainerType createSpecContainerType,
                    CreateSpecReferenceDefinition createSpecReference,
                    CreateSpecContainerFactoryInstanceHolderDefinition createSpecContainerFactoryInstanceHolder,
                    CreateSpecContainerFactoryMethodDefinition createSpecContainerFactoryMethod,
                    CreateSpecContainerBuilderMethodDefinition createSpecContainerBuilderMethod
            ) {
                this.createSpecContainerType = createSpecContainerType;
                this.createSpecReference = createSpecReference;
                this.createSpecContainerFactoryInstanceHolder = createSpecContainerFactoryInstanceHolder;
                this.createSpecContainerFactoryMethod = createSpecContainerFactoryMethod;
                this.createSpecContainerBuilderMethod = createSpecContainerBuilderMethod;
            }

            public SpecContainerDefinition Build(
                    SpecDescriptor specDescriptor,
                    InjectorDescriptor injectorDescriptor,
                    IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
            ) {
                var specContainerType = createSpecContainerType(
                        injectorDescriptor.InjectorType,
                        specDescriptor.SpecType);
                var specReference = createSpecReference(specDescriptor);

                var instanceHolders = new List<SpecContainerFactoryInstanceHolderDefinition>();
                var factoryMethods = new List<SpecContainerFactoryMethodDefinition>();
                var builderMethods = new List<SpecContainerBuilderMethodDefinition>();



                return null!;
            }
        }
    }
}
