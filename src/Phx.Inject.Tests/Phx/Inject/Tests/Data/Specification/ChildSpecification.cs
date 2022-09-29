// -----------------------------------------------------------------------------
//  <copyright file="IChildInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    internal static class GrandchildSpecification {
        [Factory]
        internal static Root GetRoot(Node node, Node secondaryNode) {
            return new Root(node, secondaryNode);
        }
    }

    [Specification]
    internal static class ChildSpecification {
        [Factory]
        internal static Node GetNode(
                [Label(ParentSpecification.LeftLeaf)] ILeaf left,
                [Label(ParentSpecification.RightLeaf)] ILeaf right) {
            return new Node(left, right);
        }
    }

    [Specification]
    internal static class ParentSpecification {
        public const string LeftLeaf = "Left";
        public const string RightLeaf = "Right";

        [Label(LeftLeaf)]
        [Factory]
        internal static ILeaf GetLeftLeaf() {
            return new StringLeaf(LeftLeaf);
        }

        [Label(RightLeaf)]
        [Factory]
        internal static ILeaf GetRightLeaf() {
            return new StringLeaf(RightLeaf);
        }
    }
}
