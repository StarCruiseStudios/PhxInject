// -----------------------------------------------------------------------------
// <copyright file="IFactory.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    /// <summary>
    ///     An interface that can be used to inject a dependency on the factory
    ///     method for a class, instead of the class itself.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFactory<T> {
        /// <summary>
        ///     Creates a new instance of type <see cref="T"/>. 
        /// </summary>
        T Create();
    }
}
