// -----------------------------------------------------------------------------
// <copyright file="LinkAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary> Models a link between one dependency key and another. </summary>
/// <remarks>
/// Link attributes establish connections in a directed acyclic graph (DAG) of dependencies.
/// They allow one specification to expose dependencies from another specification under a
/// different type or qualifier.
///
/// ## Mutual Exclusivity Constraints
///
/// For both input and output:
/// - Either a <see cref="LabelAttribute"/> (string-based) or <see cref="QualifierAttribute"/> 
///   (type-based) qualifier may be specified, but not both
/// - If neither is specified, the dependency is matched by type alone
///
/// ## Example
///
/// <code>
/// [Link(typeof(IService), typeof(IMyService))]
/// [Specification]
/// public class MySpec { }
/// </code>
///
/// This links IService from a dependency to IMyService exposed by this specification.
/// </remarks>
/// <seealso cref="LabelAttribute"/>
/// <seealso cref="QualifierAttribute"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
public class LinkAttribute : Attribute {
    /// <summary> The dependency key for the type consumed by the link. </summary>
    public Type Input { get; }

    /// <summary> The dependency key for the type exposed by the link. </summary>
    public Type Output { get; }
    
    /// <summary>
    /// An optional <see cref="LabelAttribute"/> qualifier for the input type. Cannot be specified at the same time as
    /// <see cref="InputQualifier"/>.
    /// </summary>
    public string? InputLabel { get; set; } = null;
    
    /// <summary>
    /// An optional <see cref="QualifierAttribute"/> qualifier for the input type. Cannot be specified at the same time as
    /// <see cref="InputLabel"/>.
    /// </summary>
    public Type? InputQualifier { get; set; } = null;
    
    /// <summary>
    /// An optional <see cref="LabelAttribute"/> qualifier for the output type. Cannot be specified at the same time as
    /// <see cref="OutputQualifier"/>.
    /// </summary>
    public string? OutputLabel { get; set; } = null;
    
    /// <summary>
    /// An optional <see cref="QualifierAttribute"/> qualifier for the output type. Cannot be specified at the same time as
    /// <see cref="OutputLabel"/>.
    /// </summary>
    public Type? OutputQualifier { get; set; } = null;

    /// <summary> Initializes a new instance of the <see cref="LinkAttribute"/> class. </summary>
    /// <param name="input"> The dependency key for the type consumed by the link. </param>
    /// <param name="output"> The dependency key for the type exposed by the link. </param>
    public LinkAttribute(Type input, Type output) {
        Input = input;
        Output = output;
    }
}
