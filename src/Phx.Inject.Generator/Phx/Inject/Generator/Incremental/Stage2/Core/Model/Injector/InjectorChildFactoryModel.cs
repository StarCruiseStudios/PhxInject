// -----------------------------------------------------------------------------
// <copyright file="InjectorChildFactoryModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

/// <summary>
///     Code generation model for a child injector factory method.
/// </summary>
/// <param name="ChildInjectorType">
///     The type metadata of the child injector interface (e.g., IRequestInjector).
/// </param>
/// <param name="ChildFactoryMethodName">
///     The method name from the parent interface (e.g., "CreateRequest").
/// </param>
/// <param name="Parameters">
///     Context parameters passed to the factory method (e.g., [HttpContext, Command]).
/// </param>
/// <param name="Location">The source location where this factory is defined.</param>
/// <remarks>
///     Stage 2 counterpart to <see cref="Stage1.Metadata.Model.Injector.InjectorChildProviderMetadata"/>.
///     Generates a method that creates hierarchical child injector instances. Each child has its own
///     specification containers (isolated singletons) but can access parent dependencies via the
///     [Dependency] interface. Parameters become constructor arguments for the child injector.
/// </remarks>
/// <param name="ChildInjectorType"> 
///     The type metadata of the child injector interface (e.g., IRequestInjector). Used to resolve
///     the child's generated implementation class name (e.g., RequestInjector_RequestSpec).
/// </param>
/// <param name="ChildFactoryMethodName"> 
///     The method name from the parent interface (e.g., "CreateRequest"). Used as-is in the
///     generated parent implementation to satisfy interface contract.
/// </param>
/// <param name="Parameters"> 
///     Context parameters passed to the factory method (e.g., [HttpContext, Command]). These become
///     constructor arguments for the child injector and are available as external dependencies in
///     the child's specifications.
/// </param>
/// <param name="Location"> The source location where this factory is defined for diagnostics. </param>
internal record InjectorChildFactoryModel(
    TypeMetadata ChildInjectorType,
    string ChildFactoryMethodName,
    IEnumerable<TypeMetadata> Parameters,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
