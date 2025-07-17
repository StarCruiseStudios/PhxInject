// -----------------------------------------------------------------------------
//  <copyright file="ParametricLeaf.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2023 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model;

internal class ParametricLeaf<T> : ILeaf {
    public T Value { get; }
    public ParametricLeaf(T value) {
        Value = value;
    }

    public static ParametricLeaf<T> Construct(T value) {
        return new ParametricLeaf<T>(value);
    }

    public override string ToString() {
        return $"ParametricLeaf({Value})";
    }
}
