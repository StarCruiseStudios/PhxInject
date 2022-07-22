// -----------------------------------------------------------------------------
//  <copyright file="Node.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data {
    public class Node {
        public ILeaf Left { get; }
        public ILeaf Right { get; }

        public Node(ILeaf left, ILeaf right) {
            Left = left;
            Right = right;
        }
    }
}