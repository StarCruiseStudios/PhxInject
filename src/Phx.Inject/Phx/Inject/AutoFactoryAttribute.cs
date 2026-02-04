// -----------------------------------------------------------------------------
//  <copyright file="FactoryAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Annotates a type or constructor that will be invoked to construct a given dependency.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor)]
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
