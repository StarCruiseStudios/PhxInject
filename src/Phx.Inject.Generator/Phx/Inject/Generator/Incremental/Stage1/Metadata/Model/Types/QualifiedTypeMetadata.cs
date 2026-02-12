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
///     Metadata representing a type with an optional qualifier.
/// </summary>
/// <param name="TypeMetadata"> The type metadata. </param>
/// <param name="QualifierMetadata"> The qualifier metadata. </param>
internal record QualifiedTypeMetadata(
    TypeMetadata TypeMetadata,
    IQualifierMetadata QualifierMetadata
) : ISourceCodeElement {
    /// <summary> Gets the source location from the underlying type metadata. </summary>
    public GeneratorIgnored<LocationInfo?> Location => TypeMetadata.Location;

    /// <summary> Returns a string representation of the qualified type. </summary>
    /// <returns> A string showing the qualifier (if present) and the type. </returns>
    public override string ToString() {
        return (QualifierMetadata is not NoQualifierMetadata)
            ? $"{QualifierMetadata} {TypeMetadata}"
            : TypeMetadata.ToString();        
    }
}
