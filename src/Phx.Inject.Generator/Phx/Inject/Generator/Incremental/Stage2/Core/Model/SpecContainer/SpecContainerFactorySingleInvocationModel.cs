// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactorySingleInvocationModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;

/// <summary>
///     Code generation model for a single factory method invocation in a specification container.
/// </summary>
/// <param name="SpecContainerType">The specification container type.</param>
/// <param name="FactoryMethodName">The name of the factory method to invoke.</param>
/// <param name="Location">The source location where this invocation is defined.</param>
/// <remarks>
///     Represents one step in a factory invocation chain. Multiple single invocations compose
///     into a chain to handle cross-container dependency resolution.
/// </remarks>
/// <param name="SpecContainerType"> The specification container type. </param>
/// <param name="FactoryMethodName"> The name of the factory method to invoke. </param>
/// <param name="Location"> The source location where this invocation is defined. </param>
internal record SpecContainerFactorySingleInvocationModel(
    TypeMetadata SpecContainerType,
    string FactoryMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
