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
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class)]
public class FactoryAttribute : Attribute {
    /// <summary>
    ///     Indicates the <see cref="FabricationMode"/> used when invoking this factory method more than
    ///     once.
    /// </summary>
    public FabricationMode FabricationMode { get; }

    /// <summary> Initializes a new instance of the <see cref="FactoryAttribute"/> class. </summary>
    /// <param name="fabricationMode">
    ///     The <see cref="FabricationMode"/> used when invoking this factory method more than once.
    ///     Defaults to <see cref="Phx.Inject.FabricationMode.Recurrent"/>.
    /// </param>
    public FactoryAttribute(FabricationMode fabricationMode = FabricationMode.Recurrent) {
        FabricationMode = fabricationMode;
    }
}
