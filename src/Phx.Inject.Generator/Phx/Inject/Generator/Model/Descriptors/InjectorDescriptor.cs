// -----------------------------------------------------------------------------
//  <copyright file="InjectorDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record InjectorDescriptor(
            TypeModel InjectorType,
            TypeModel InjectorInterface,
            IEnumerable<InjectorProviderMethodDescriptor> Providers,
            IEnumerable<InjectorBuilderMethodDescriptor> Builders,
            IEnumerable<SpecDescriptor> Specifications,
            Location Location) : IDescriptor;
}
