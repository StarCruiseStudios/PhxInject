// -----------------------------------------------------------------------------
// <copyright file="LabelQualifierMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

/// <summary>
///     Metadata for the user-declared <c>[Label("name")]</c> attribute.
///     See <see cref="Phx.Inject.LabelAttribute"/>.
/// </summary>
/// <param name="LabelAttributeMetadata">The metadata extracted from the [Label] attribute.</param>
/// <remarks>
///     Label string matching is case-sensitive. Empty strings are valid but discouraged.
///     Use <see cref="CustomQualifierMetadata"/> when type-safety is needed.
/// </remarks>
internal sealed record LabelQualifierMetadata(
    LabelAttributeMetadata LabelAttributeMetadata
) : IQualifierMetadata {
    /// <summary>
    ///     Gets the source location where the [Label] attribute was declared.
    /// </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = LabelAttributeMetadata.Location;
    
    /// <summary>
    ///     Returns a diagnostic-friendly representation showing the label value.
    /// </summary>
    /// <returns>String in the format "[Label(labelValue)]" for error messages and logs.</returns>
    public override string ToString() {
        return $"[Label({LabelAttributeMetadata.Label})]";        
    }

    /// <summary>
    ///     Determines whether another qualifier is a label with the same string value.
    /// </summary>
    public bool Equals(IQualifierMetadata? other) {
        if (other is not LabelQualifierMetadata l) return false;
        if (ReferenceEquals(this, l)) return true;
        return LabelAttributeMetadata.Equals(l.LabelAttributeMetadata);
    }

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode() {
        return LabelAttributeMetadata.GetHashCode();
    }
}
