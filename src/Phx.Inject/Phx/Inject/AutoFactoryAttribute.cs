// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Annotates a type or constructor that will be automatically used to construct a given dependency.
/// </summary>
/// <remarks>
/// Auto-factories enable the generator to automatically create factory logic without requiring
/// explicit factory methods. When applied to a type, the generator analyzes the type's constructors
/// and dependencies to generate appropriate factory code.
///
/// ## Difference from FactoryAttribute
///
/// - <see cref="FactoryAttribute"/> requires explicit factory methods in specifications
/// - <see cref="AutoFactoryAttribute"/> allows the generator to infer construction logic
///   from the type's constructor signature
///
/// ## Usage
///
/// Apply to a class to automatically generate factory logic:
///
/// <code>
/// [AutoFactory]
/// public class MyService {
///     public MyService(ILogger logger) { }
/// }
/// </code>
///
/// The generator creates a factory that resolves the ILogger dependency and constructs MyService.
/// </remarks>
/// <seealso cref="FactoryAttribute"/>
/// <seealso cref="FabricationMode"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor, Inherited = false)]
public class AutoFactoryAttribute : Attribute {
    /// <summary>
    ///     Indicates the <see cref="FabricationMode"/> used when invoking this factory method more than
    ///     once.
    /// </summary>
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Recurrent;
    
    /// <summary> Initializes a new instance of the <see cref="AutoFactoryAttribute"/> class. </summary>
    public AutoFactoryAttribute() { }
    
    /// <summary> Initializes a new instance of the <see cref="AutoFactoryAttribute"/> class. </summary>
    /// <param name="fabricationMode">
    ///     The <see cref="FabricationMode"/> used when invoking this factory method more than once.
    /// </param>
    public AutoFactoryAttribute(FabricationMode fabricationMode) {
        FabricationMode = fabricationMode;
    }
}
