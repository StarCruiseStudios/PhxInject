// -----------------------------------------------------------------------------
// <copyright file="Factory.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     A type that can be used to inject a dependency on the factory method for a class instead of the
///     class itself.
/// </summary>
/// <typeparam name="T"> The type of the dependency. </typeparam>
public sealed class Factory<T> {
    private readonly Func<T> factory;

    /// <summary> Initializes a new instance of the <see cref="Factory"/> class. </summary>
    /// <param name="factory"> The factory method used to construct a new instance of the dependency class. </param>
    public Factory(Func<T> factory) {
        this.factory = factory;
    }

    /// <summary> Creates a new instance of the dependency. </summary>
    /// <returns> A new instance of type <see cref="T"/>. </returns>
    public T Create() {
        return factory();
    }
}
