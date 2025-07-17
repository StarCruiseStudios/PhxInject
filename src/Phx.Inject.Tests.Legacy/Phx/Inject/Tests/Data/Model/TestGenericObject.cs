// -----------------------------------------------------------------------------
// <copyright file="TestGenericObject.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model;

public class TestGenericObject<T> {
    public T Value { get; }

    public TestGenericObject(T value) {
        Value = value;
    }
}
