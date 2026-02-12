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
///     Model representing a provider method in an injector.
/// </summary>
/// <param name="ProvidedType"> The type provided by the method. </param>
/// <param name="ProviderMethodName"> The name of the provider method. </param>
/// <param name="Location"> The source location where this provider is defined. </param>
internal record InjectorProviderModel(
    QualifiedTypeMetadata ProvidedType,
    string ProviderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
