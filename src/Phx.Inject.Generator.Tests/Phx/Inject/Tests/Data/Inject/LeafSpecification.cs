// -----------------------------------------------------------------------------
//  <copyright file="LeafSpecification.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2023 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Tests.Data.Model;

namespace Phx.Inject.Tests.Data.Inject;

[Specification]
internal static class LeafSpecification {
    [Factory(FabricationMode.Scoped)]
    internal static IntLeaf GetIntLeaf() {
        return new IntLeaf(10);
    }

    [Factory]
    internal static StringLeaf GetStringLeaf() {
        return new StringLeaf("Hello");
    }

    [Factory]
    internal static ParametricLeaf<int> GetParametricLeaf() {
        return new ParametricLeaf<int>(15);
    }
}

[Specification]
[Link(typeof(StringLeaf), typeof(ILeaf))]
internal static class LeafLinks { }
