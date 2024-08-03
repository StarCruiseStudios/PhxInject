// -----------------------------------------------------------------------------
// <copyright file="PartialAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    /// <summary>
    ///     Annotates a factory method as a partial factory. This can be used on
    ///     a factory that returns a List, Set, or Dictionary to indicate that
    ///     multiple factories with the same type and qualifiers should be
    ///     combined into a single dependency.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class PartialAttribute : Attribute { }
}
