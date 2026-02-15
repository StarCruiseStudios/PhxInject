// -----------------------------------------------------------------------------
// <copyright file="CustomQualifierMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for user-defined qualifier attributes marked with <c>[Qualifier]</c>.
///     See <see cref="Phx.Inject.QualifierAttribute"/>.
/// </summary>
/// <param name="QualifierAttributeMetadata">The metadata describing the custom qualifier attribute type.</param>
/// <remarks>
///     Custom qualifiers provide type-safe disambiguation compared to string labels.
///     Properties on the qualifier attribute are not captured - it's treated as a marker only.
/// </remarks>
internal sealed record CustomQualifierMetadata(
    QualifierAttributeMetadata QualifierAttributeMetadata
) : IQualifierMetadata {
    /// <summary>
    ///     Gets the source location where the custom qualifier attribute was applied.
    /// </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = QualifierAttributeMetadata.Location;
    
    /// <summary>
    ///     Returns a diagnostic representation showing the qualifier attribute type.
    /// </summary>
    /// <returns>
    ///     String in the format "[@QualifierType]" for error messages and logs.
    /// </returns>
    public override string ToString() {
        return $"[@{QualifierAttributeMetadata.QualifierType}]";        
    }

    /// <summary>
    ///     Determines whether another qualifier is a custom qualifier of the same attribute type.
    /// </summary>
    public bool Equals(IQualifierMetadata? other) {
        if (other is not CustomQualifierMetadata o) return false;
        if (ReferenceEquals(this, o)) return true;
        return QualifierAttributeMetadata.Equals(o.QualifierAttributeMetadata);
    }

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode() {
        return QualifierAttributeMetadata.GetHashCode();
    }
}
