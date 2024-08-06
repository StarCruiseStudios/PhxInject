// -----------------------------------------------------------------------------
// <copyright file="RuntimeFactorySpecification.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    internal static class RuntimeFactorySpecification {
        [Factory]
        internal static ILeaf GetLeaf() {
            return new IntLeaf(10);
        }

        [Factory]
        [Label("LabeledLeaf")]
        internal static ILeaf GetLabeledLeaf() {
            return new IntLeaf(42);
        }

        [Factory]
        internal static LeafFactory GetLeafFactory(Factory<ILeaf> factory) {
            return new LeafFactory(factory.Create);
        }

        [Factory]
        [Label("LabeledLeaf")]
        internal static LeafFactory GetLabeledLeafFactory([Label("LabeledLeaf")] Factory<ILeaf> factory) {
            return new LeafFactory(factory.Create);
        }
    }
}
