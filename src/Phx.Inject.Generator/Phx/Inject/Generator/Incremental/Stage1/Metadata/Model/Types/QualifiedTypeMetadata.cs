// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

/// <summary>
///     Pairs a type identity with a qualifier for dependency binding disambiguation.
/// </summary>
/// <param name="TypeMetadata">The underlying type identity.</param>
/// <param name="QualifierMetadata">The qualifier that disambiguates this binding.</param>
/// <remarks>
///     Enables multiple bindings of the same type (e.g., @Primary ILogger vs @Backup ILogger).
///     The qualifier is part of the binding's identity during resolution.
/// </remarks>
internal record QualifiedTypeMetadata(
    TypeMetadata TypeMetadata,
    IQualifierMetadata QualifierMetadata
) : ISourceCodeElement {
    /// <summary>
    ///     Gets the source location from the underlying type.
    /// </summary>
    public GeneratorIgnored<LocationInfo?> Location => TypeMetadata.Location;

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString() {
        return (QualifierMetadata is not NoQualifierMetadata)
            ? $"{QualifierMetadata} {TypeMetadata}"
            : TypeMetadata.ToString();        
    }
}
