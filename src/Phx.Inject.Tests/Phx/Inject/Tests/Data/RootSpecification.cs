// -----------------------------------------------------------------------------
//  <copyright file="RootSpecification.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data {
    [Specification]
    internal static class RootSpecification {
        [Factory(FabricationMode.Scoped)]
        internal static Root GetRoot(Node node) {
            return new Root(node);
        }

        [Factory]
        internal static Node GetNode(IntLeaf left, ILeaf right) {
            return new Node(left, right);
        }
    }
}
