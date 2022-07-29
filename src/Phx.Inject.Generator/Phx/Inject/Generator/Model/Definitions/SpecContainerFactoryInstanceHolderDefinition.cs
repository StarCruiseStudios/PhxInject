// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryInstanceHolderDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate SpecContainerFactoryInstanceHolderDefinition CreateSpecContainerFactoryInstanceHolderDefinition(
            SpecFactoryMethodDescriptor factoryDescriptor
    );

    internal record SpecContainerFactoryInstanceHolderDefinition(
            TypeModel HeldInstanceType,
            string ReferenceName,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateInstanceHolderName createInstanceHolderName;

            public Builder(CreateInstanceHolderName createInstanceHolderName) {
                this.createInstanceHolderName = createInstanceHolderName;
            }

            public SpecContainerFactoryInstanceHolderDefinition Build(SpecFactoryMethodDescriptor factoryDescriptor) {

                return new SpecContainerFactoryInstanceHolderDefinition(
                        factoryDescriptor.ReturnType.TypeModel,
                        createInstanceHolderName(factoryDescriptor.ReturnType),
                        factoryDescriptor.Location);
            }
        }
    }
}
