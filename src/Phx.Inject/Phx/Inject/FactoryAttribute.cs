// -----------------------------------------------------------------------------
// <copyright file="FactoryAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Annotates a factory method that will be invoked to construct a given dependency.
/// </summary>
/// <remarks>
/// Factory methods define how to create instances of dependencies. The method's return type
/// determines what dependency type it provides, and its parameters specify the dependencies
/// it requires.
///
/// ## FabricationMode Behavior
///
/// The <see cref="FabricationMode"/> property controls how instances are created across
/// multiple invocations of the same factory:
///
/// - **Recurrent**: Each invocation creates a new instance
/// - **Scoped**: Returns the same instance within a request scope
/// - **Singleton**: Returns the same instance for the injector's lifetime
/// </remarks>
/// <seealso cref="AutoFactoryAttribute"/>
/// <seealso cref="FabricationMode"/>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public class FactoryAttribute : Attribute {
    /// <summary>
    ///     Indicates the <see cref="FabricationMode"/> used when invoking this factory method more than
    ///     once.
    /// </summary>
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Recurrent;
    
    /// <summary> Initializes a new instance of the <see cref="FactoryAttribute"/> class. </summary>
    public FactoryAttribute() { }
    
    /// <summary> Initializes a new instance of the <see cref="FactoryAttribute"/> class. </summary>
    /// <param name="fabricationMode">
    ///     The <see cref="FabricationMode"/> used when invoking this factory method more than once.
    /// </param>
    public FactoryAttribute(FabricationMode fabricationMode) {
        FabricationMode = fabricationMode;
    }
}
