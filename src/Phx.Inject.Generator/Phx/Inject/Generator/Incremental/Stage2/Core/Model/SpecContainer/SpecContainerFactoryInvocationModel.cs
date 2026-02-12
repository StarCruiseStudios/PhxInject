// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryInvocationModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing an invocation of a specification container factory method.
/// </summary>
/// <param name="FactoryInvocationDefs"> The factory invocations to chain together. </param>
/// <param name="FactoryReturnType"> The return type of the factory invocation. </param>
/// <param name="RuntimeFactoryProvidedType"> The optional runtime-provided type for the factory. </param>
/// <param name="Location"> The source location where this invocation is defined. </param>
internal record SpecContainerFactoryInvocationModel(
    EquatableList<SpecContainerFactorySingleInvocationModel> FactoryInvocationDefs,
    QualifiedTypeMetadata FactoryReturnType,
    TypeMetadata? RuntimeFactoryProvidedType,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
