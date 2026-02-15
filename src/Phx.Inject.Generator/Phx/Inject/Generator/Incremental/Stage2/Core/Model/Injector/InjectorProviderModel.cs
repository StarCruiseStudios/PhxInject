// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderModel.cs" company="Star Cruise Studios LLC">
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
///     Code generation model for a provider method in the injector class.
/// </summary>
/// <param name="ProvidedType">
///     The qualified type returned by this provider, including any [Label] qualifiers.
/// </param>
/// <param name="ProviderMethodName">
///     The method name from the user's interface (e.g., "GetPrimaryDatabase").
/// </param>
/// <param name="Location">The source location where this provider is defined.</param>
/// <remarks>
///     Stage 2 counterpart to <see cref="Stage1.Metadata.Model.Injector.InjectorProviderMetadata"/>,
///     enriched with resolution strategy. Generates a method that delegates to a specification
///     container's factory method, returning the constructed instance.
/// </remarks>
/// <param name="ProvidedType"> 
///     The qualified type returned by this provider, including any [Label] qualifiers. Used to resolve
///     the correct specification factory method during code generation.
/// </param>
/// <param name="ProviderMethodName"> 
///     The method name from the user's interface (e.g., "GetPrimaryDatabase"). Used as-is in the
///     generated implementation to satisfy interface contract.
/// </param>
/// <param name="Location"> The source location where this provider is defined for diagnostics. </param>
internal record InjectorProviderModel(
    QualifiedTypeMetadata ProvidedType,
    string ProviderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
