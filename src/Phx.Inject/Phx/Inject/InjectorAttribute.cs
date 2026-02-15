// -----------------------------------------------------------------------------
// <copyright file="InjectorAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary> Annotates an injector interface as the entry point to a dependency graph. </summary>
/// <remarks>
/// This attribute marks an interface that will have a concrete injector implementation
/// generated at compile time. The injector provides access to all factories and builders
/// defined in the specified specifications.
///
/// ## Usage
///
/// Apply this attribute to an interface to generate an injector:
///
/// <code>
/// [Injector(typeof(MySpecification))]
/// public interface IMyInjector {
///     MyService GetService();
/// }
/// </code>
/// </remarks>
/// <seealso cref="SpecificationAttribute"/>
/// <seealso cref="DependencyAttribute"/>
[AttributeUsage(AttributeTargets.Interface)]
public class InjectorAttribute : Attribute {
    /// <summary> The name to use for the generated injector class. </summary>
    /// <remarks>
    ///     This value may be <see langword="null" /> if no custom class name is specified. If no 
    ///     custom name is specified, a default value of "GeneratedXyz" will be used, where Xyz is 
    ///     the annotated interface's name with the leading "I" removed, if present.
    /// </remarks>
    public string? GeneratedClassName { get; set; } = null;

    /// <summary> Gets a collection of specification types used by this injector. </summary>
    /// <value>
    ///     An enumerable of specification types. This collection defines which specifications
    ///     provide factories and builders for this injector.
    /// </value>
    public IEnumerable<Type> Specifications { get; }

    /// <summary> Initializes a new instance of the <see cref="InjectorAttribute"/> class. </summary>
    /// <param name="specifications"> A collection of specification types used by this injector. </param>
    public InjectorAttribute(params Type[] specifications) {
        Specifications = specifications;
    }
    
    /// <summary> Initializes a new instance of the <see cref="InjectorAttribute"/> class. </summary>
    /// <param name="generatedClassName"> The name to use for the generated injector class. </param>
    /// <param name="specifications"> A collection of specification types used by this injector. </param>
    public InjectorAttribute(string? generatedClassName, params Type[] specifications) {
        GeneratedClassName = generatedClassName;
        Specifications = specifications;
    }
}
