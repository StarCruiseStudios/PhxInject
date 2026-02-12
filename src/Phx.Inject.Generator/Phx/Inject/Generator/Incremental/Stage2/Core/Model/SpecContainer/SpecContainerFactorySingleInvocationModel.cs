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
///     Model representing a single factory method invocation in a specification container.
/// </summary>
/// <remarks>
///     <para>Atomic Factory Call:</para>
///     <para>
///     Represents one step in a factory invocation chain. Each single invocation specifies exactly
///     which container and which method to call. Multiple SingleInvocationModels compose into a
///     SpecContainerFactoryInvocationModel to handle cross-container dependency resolution.
///     </para>
///     
///     <para>Parameter Passing Pattern:</para>
///     <para>
///     The generated code calls the FactoryMethodName on the SpecContainerType, passing no
///     explicit arguments (all dependency resolution happens recursively within that factory).
///     This simplifies code generation and enables clean separation between containers.
///     </para>
///     
///     <para>Generated Code Example:</para>
///     <code>
///     // SpecContainerType = "MySpec_Container"
///     // FactoryMethodName = "CreateLogger"
///     // Generates:
///     mySpecContainer.CreateLogger()
///     </code>
/// </remarks>
/// <param name="SpecContainerType"> The specification container type. </param>
/// <param name="FactoryMethodName"> The name of the factory method to invoke. </param>
/// <param name="Location"> The source location where this invocation is defined. </param>
internal record SpecContainerFactorySingleInvocationModel(
    TypeMetadata SpecContainerType,
    string FactoryMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
