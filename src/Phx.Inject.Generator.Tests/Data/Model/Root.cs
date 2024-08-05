// -----------------------------------------------------------------------------
//  <copyright file="Root.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model {
    public class Root {
        public Node Node { get; }

        public Node SecondaryNode { get; }

        public Root(Node node, Node secondaryNode) {
            Node = node;
            SecondaryNode = secondaryNode;
        }
    }
}
