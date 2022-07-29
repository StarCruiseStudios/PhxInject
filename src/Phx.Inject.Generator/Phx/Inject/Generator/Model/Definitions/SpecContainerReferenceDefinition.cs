// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerReferenceDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate SpecContainerReferenceDefinition CreateSpecContainerReferenceDefinition(
            InjectorDescriptor injectorDescriptor,
            SpecDescriptor specDescriptor
    );

    internal record SpecContainerReferenceDefinition(
            TypeModel SpecContainerType,
            string ReferenceName,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateSpecContainerType createSpecContainerType;

            public Builder(CreateSpecContainerType createSpecContainerType) {
                this.createSpecContainerType = createSpecContainerType;
            }

            public SpecContainerReferenceDefinition Build(
                    InjectorDescriptor injectorDescriptor,
                    SpecDescriptor specDescriptor
            ) {
                var specContainerType = createSpecContainerType(injectorDescriptor.InjectorType, specDescriptor.SpecType);
                return new SpecContainerReferenceDefinition(
                        specContainerType,
                        specContainerType.TypeName,
                        injectorDescriptor.Location);
            }
        }
    }
}
