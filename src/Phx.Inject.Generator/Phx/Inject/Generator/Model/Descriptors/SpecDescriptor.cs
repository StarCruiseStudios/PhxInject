// -----------------------------------------------------------------------------
//  <copyright file="SpecDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record SpecDescriptor(
            TypeModel SpecType,
            SpecInstantiationMode InstantiationMode,
            IEnumerable<SpecFactoryDescriptor> Factories,
            IEnumerable<SpecBuilderDescriptor> Builders,
            IEnumerable<LinkDescriptor> Links,
            Location Location) : IDescriptor;
}
