// -----------------------------------------------------------------------------
// <copyright file="ContainerSpecification.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    internal static class ContainerSpecification {
        private static int currentInt = 0;
        
        
        [Factory(FabricationMode.ContainerScoped)]
        internal static int GetInt() {
            return currentInt++;
        }
        
        [Factory(FabricationMode.Scoped)]
        internal static StringLeaf GetStringLeaf(int value) {
            return new StringLeaf(value.ToString());
        }

        [Factory]
        [Partial]
        internal static List<IntLeaf> GetIntLeaf1(IntLeaf leaf) {
            return new List<IntLeaf> { leaf };
        }
        
        [Factory]
        [Partial]
        internal static List<IntLeaf> GetIntLeaf2(IntLeaf leaf) {
            return new List<IntLeaf> { leaf };
        }

        [Factory(FabricationMode.Container)]
        internal static Node GetNode(List<IntLeaf> leaves) {
            var left = leaves[0];
            var right = leaves[1];
            return new Node(left, right);
        }
        
        [Factory(FabricationMode.Container)]
        [Label("WithScoped")]
        internal static Node GetNode2(IntLeaf intLeaf, StringLeaf stringLeaf) {
            return new Node(intLeaf, stringLeaf);
        }
    }
}
