// -----------------------------------------------------------------------------
// <copyright file="InjectorChildProviderMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Metadata for a child provider method that creates hierarchical sub-container instances.
/// </summary>
/// <param name="ChildProviderMethodName">The method name from the parent interface.</param>
/// <param name="ChildInjectorType">The type metadata of the child injector interface.</param>
/// <param name="Parameters">Context parameters passed to the child factory method, providing external dependencies.</param>
/// <param name="ChildInjectorAttribute">The [ChildInjector] attribute metadata.</param>
/// <param name="Location">The source location for diagnostics.</param>
/// <remarks>
///     Child providers create scoped sub-containers with their own isolated state. Enables
///     hierarchical scopes (Application → Request → Operation) where children can access parent
///     dependencies via [Dependency] interface. Marked with [ChildInjector] attribute.
/// </remarks>
internal record InjectorChildProviderMetadata(
    string ChildProviderMethodName,
    TypeMetadata ChildInjectorType,
    EquatableList<TypeMetadata> Parameters,
    ChildInjectorAttributeMetadata ChildInjectorAttribute,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }
