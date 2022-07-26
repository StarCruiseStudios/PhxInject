// -----------------------------------------------------------------------------
//  <copyright file="LeafSpecification.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;
    
    [Specification]
    internal static class LeafSpecification {
        [Factory(FabricationMode.Scoped)]
        internal static IntLeaf GetIntLeaf() {
            return new IntLeaf(10);
        }

        [Factory(FabricationMode.Recurrent)]
        internal static StringLeaf GetStringLeaf() {
            return new StringLeaf("Hello");
        }
    }

    [Specification]
    [Link(typeof(StringLeaf), typeof(ILeaf))]
    internal static class LeafLinks { }
}
