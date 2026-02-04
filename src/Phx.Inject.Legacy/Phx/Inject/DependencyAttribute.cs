// -----------------------------------------------------------------------------
// <copyright file="DependencyAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary> Defines an dependency that is required by an injector interface. </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class DependencyAttribute : Attribute {
    /// <summary> Gets the type of the dependency. </summary>
    public Type DependencyType { get; }

    /// <summary> Initialzes a new instance of the <see cref="DependencyAttribute"/> class. </summary>
    /// <param name="dependencyType"> The type of the dependency. </param>
    public DependencyAttribute(Type dependencyType) {
        DependencyType = dependencyType;
    }
}
