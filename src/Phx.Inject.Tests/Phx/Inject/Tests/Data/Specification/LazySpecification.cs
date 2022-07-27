// -----------------------------------------------------------------------------
//  <copyright file="LazySpecification.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    internal static class LazySpecification {
        [Builder]
        internal static void BuildLazyType(LazyType type, ILeaf leaf) {
            type.Value = leaf;
        }
    }
}
