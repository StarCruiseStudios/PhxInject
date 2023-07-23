// -----------------------------------------------------------------------------
//  <copyright file="BuilderAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    using System;

    /// <summary>
    ///     Annotates a builder method that will be invoked to complete the construction of a given
    ///     dependency.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BuilderAttribute : Attribute { }
}
