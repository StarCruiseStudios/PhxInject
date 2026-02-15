// -----------------------------------------------------------------------------
// <copyright file="DependencyAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Defines a dependency that is required by an injector interface.
/// </summary>
/// <remarks>
/// This attribute is applied to an injector interface to declare that it requires
/// a specific dependency type to be available. The dependency must be provided by
/// a linked specification or parent injector.
///
/// ## Usage
///
/// Apply this attribute when an injector needs access to a dependency that is not
/// directly provided by its own specifications:
///
/// <code>
/// [Injector(typeof(MySpec))]
/// [Dependency(typeof(ILogger))]
/// public interface IMyInjector { }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Interface)]
public class DependencyAttribute : Attribute {
    /// <summary> Gets the type of the dependency. </summary>
    public Type DependencyType { get; }

    /// <summary> Initializes a new instance of the <see cref="DependencyAttribute"/> class. </summary>
    /// <param name="dependencyType"> The type of the dependency. </param>
    public DependencyAttribute(Type dependencyType) {
        DependencyType = dependencyType;
    }
}
