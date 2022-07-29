﻿// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerCollectionDefinition.cs" company="Star Cruise Studios LLC">
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
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate SpecContainerCollectionDefinition CreateSpecContainerCollectionDefinition(
            InjectorDescriptor injectorDescriptor);

    internal record SpecContainerCollectionDefinition(
            IEnumerable<SpecContainerReferenceDefinition> SpecContainerReferences,
            Location Location
    ) : IDefinition {
        public class Builder {
            private readonly CreateSpecContainerReferenceDefinition createSpecContainerReference;

            public Builder(CreateSpecContainerReferenceDefinition createSpecContainerReference) {
                this.createSpecContainerReference = createSpecContainerReference;
            }

            public SpecContainerCollectionDefinition Build(InjectorDescriptor injectorDescriptor) {
                var specContainerReferences = injectorDescriptor.Specifications.Select(
                                specDescriptor => createSpecContainerReference(injectorDescriptor, specDescriptor))
                        .ToImmutableList();
                return new SpecContainerCollectionDefinition(
                        specContainerReferences,
                        injectorDescriptor.Location);
            }
        }
    }
}
