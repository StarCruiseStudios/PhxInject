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
///     Metadata representing a label qualifier annotation.
/// </summary>
/// <param name="LabelAttributeMetadata"> The [Label] attribute metadata. </param>
internal record LabelQualifierMetadata(
    LabelAttributeMetadata LabelAttributeMetadata
) : IQualifierMetadata {
    /// <summary> Gets the source location of the label qualifier. </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = LabelAttributeMetadata.Location;
    
    /// <summary> Returns a string representation of the label qualifier. </summary>
    /// <returns> A string in the format [Label(labelValue)]. </returns>
    public override string ToString() {
        return $"[Label({LabelAttributeMetadata.Label})]";        
    }

    /// <summary> Compares this label qualifier with another for equality. </summary>
    /// <param name="other"> The other qualifier to compare with. </param>
    /// <returns> True if the qualifiers are equal, false otherwise. </returns>
    public virtual bool Equals(IQualifierMetadata? other) {
        if (other is not LabelQualifierMetadata l) return false;
        if (ReferenceEquals(this, l)) return true;
        return Location.Equals(l.Location) && LabelAttributeMetadata.Equals(l.LabelAttributeMetadata);
    }

    public override int GetHashCode() {
        unchecked {
            return (Location.GetHashCode() * 397) ^ LabelAttributeMetadata.GetHashCode();
        }
    }
}
