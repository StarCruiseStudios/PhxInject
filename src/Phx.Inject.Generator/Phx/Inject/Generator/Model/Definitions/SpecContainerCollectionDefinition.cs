// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerCollectionDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record SpecContainerCollectionDefinition(
            IEnumerable<SpecContainerReferenceDefinition> SpecContainerReferences,
            Location Location
    ) : IDefinition;
}
