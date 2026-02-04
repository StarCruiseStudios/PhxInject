// -----------------------------------------------------------------------------
// <copyright file="LazyType.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model;

public class LazyType {
    public ILeaf Value { get; set; } = null!;

    public static void Inject(LazyType target, ILeaf value) {
        target.Value = value;
    }
}
