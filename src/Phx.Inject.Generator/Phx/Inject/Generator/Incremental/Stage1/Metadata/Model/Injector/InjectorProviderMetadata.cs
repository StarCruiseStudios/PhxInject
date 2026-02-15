// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for a provider method that returns constructed object instances.
/// </summary>
/// <param name="ProviderMethodName">The method name from the user's interface.</param>
/// <param name="ProvidedType">The qualified type returned by this provider, including any [Label] qualifiers.</param>
/// <param name="Location">The source location for diagnostics.</param>
/// <remarks>
///     Provider methods are factory methods on the injector interface. Distinguished from activators
///     (void return) and child providers (returns child injector interface).
/// </remarks>
internal record InjectorProviderMetadata(
    string ProviderMethodName,
    QualifiedTypeMetadata ProvidedType,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }
