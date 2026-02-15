// -----------------------------------------------------------------------------
// <copyright file="InjectorActivatorMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Metadata for an activator method that initializes existing object instances.
/// </summary>
/// <param name="ActivatorMethodName">The method name from the user's interface.</param>
/// <param name="ActivatedType">The qualified type to be initialized, including any [Label] qualifiers.</param>
/// <param name="Location">The source location for diagnostics.</param>
/// <remarks>
///     Activator methods accept an existing object and inject its dependencies (void return).
///     Corresponds to [Builder] methods in specifications. Used for post-construction initialization
///     when object creation is external to the DI container.
/// </remarks>
internal record InjectorActivatorMetadata(
    string ActivatorMethodName,
    QualifiedTypeMetadata ActivatedType,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }