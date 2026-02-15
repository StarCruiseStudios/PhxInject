// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryRequiredPropertyModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;

/// <summary>
///     Code generation model for a required property initialization in a factory method.
/// </summary>
/// <param name="PropertyName">The name of the property to set.</param>
/// <param name="Value">The factory invocation that provides the property value.</param>
/// <param name="Location">The source location where this property is defined.</param>
/// <remarks>
///     Enables C# object initializer syntax for types with `required` properties. Properties are
///     initialized after constructor execution, enabling proper initialization order.
/// </remarks>
/// <param name="PropertyName"> The name of the property to set. </param>
/// <param name="Value"> The factory invocation that provides the property value. </param>
/// <param name="Location"> The source location where this property is defined. </param>
internal record SpecContainerFactoryRequiredPropertyModel(
    string PropertyName,
    SpecContainerFactoryInvocationModel Value,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
