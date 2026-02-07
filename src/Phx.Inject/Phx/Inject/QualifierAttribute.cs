// -----------------------------------------------------------------------------
// <copyright file="QualifierAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Annotates an attribute as a qualifier that can be applied to a factory method or dependency as
///     a unique label used to discriminate them from other dependencies with the same type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class QualifierAttribute : Attribute {
    /// <summary> The <see cref="AttributeTargets"/> flags applied to a qualifier attribute. </summary>
    public const AttributeTargets Usage =
        AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Interface
        | AttributeTargets.Constructor
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Parameter
        | AttributeTargets.Field;
}
