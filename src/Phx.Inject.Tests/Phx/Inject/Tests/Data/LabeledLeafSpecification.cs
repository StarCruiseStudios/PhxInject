// -----------------------------------------------------------------------------
//  <copyright file="LabeledLeafSpecification.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data {
    // [Specification]
    internal static class LabeledLeafSpecification {
        public const string DefaultLeafData = "Default";
        public const string NonDefaultLeafAData = "NonDefaultLeafA";
        public const string NonDefaultLeafBData = "NonDefaultLeafB";
        public const string AttributeNamedLeafAData = "AttributeNamedLeafA";
        public const string AttributeNamedLeafBData = "AttributeNamedLeafB";
        public const string StringNamedLeafAData = "StringNamedLeafA";
        public const string NamedStringLeafData = "NamedStringLeaf";

        // [Factory]
        // internal static ILeaf GetDefaultLeaf()
        // {
        //     return new StringLeaf(DefaultLeafData);
        // }

        [Label("NonDefaultLeafA")]
        [Factory]
        internal static ILeaf GetNonDefaultLeafA() {
            return new StringLeaf(NonDefaultLeafAData);
        }

        [Label("NonDefaultLeafB")]
        [Factory]
        internal static ILeaf GetNonDefaultLeafB() {
            return new StringLeaf(NonDefaultLeafBData);
        }

        [NamedLeafA]
        [Factory]
        internal static ILeaf GetAttributeNamedLeafA() {
            return new StringLeaf(AttributeNamedLeafAData);
        }

        [NamedLeafB]
        [Factory]
        internal static ILeaf GetAttributeNamedLeafB() {
            return new StringLeaf(AttributeNamedLeafBData);
        }

        [Label("NamedLeafA")]
        [Factory]
        internal static ILeaf GetStringNamedLeafA() {
            return new StringLeaf(StringNamedLeafAData);
        }

        [Label("NamedLeafA")]
        [Factory]
        internal static StringLeaf GetNamedStringLeaf() {
            return new StringLeaf(NamedStringLeafData);
        }

        internal static Node GetNode(
            [Label("NonDefaultLeafA")] ILeaf left,
            [NamedLeafA] ILeaf right
        ) {
            return new Node(left, right);
        }

        [Label("NonDefaultLeafA")]
        [Builder]
        internal static void BuildLazyTypeLeafA(LazyType type, [Label("NonDefaultLeafA")] ILeaf leaf) {
            type.Value = leaf;
        }

        [Builder]
        internal static void BuildLazyTypeDefault(LazyType type, ILeaf leaf) {
            type.Value = leaf;
        }
    }
}
