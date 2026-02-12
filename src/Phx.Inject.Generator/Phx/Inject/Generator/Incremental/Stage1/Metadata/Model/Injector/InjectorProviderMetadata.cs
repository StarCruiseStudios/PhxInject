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
///     Metadata representing an analyzed injector provider method.
/// </summary>
/// <param name="ProviderMethodName"> The name of the provider method. </param>
/// <param name="ProvidedType"> The qualified type that is provided. </param>
/// <param name="Location"> The source location of the provider definition. </param>
internal record InjectorProviderMetadata(
    string ProviderMethodName,
    QualifiedTypeMetadata ProvidedType,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }
