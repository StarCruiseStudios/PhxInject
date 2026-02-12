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
///     Metadata representing a custom qualifier annotation.
/// </summary>
/// <param name="QualifierAttributeMetadata"> The [Qualifier] attribute metadata. </param>
internal record CustomQualifierMetadata(
    QualifierAttributeMetadata QualifierAttributeMetadata
) : IQualifierMetadata {
    /// <summary> Gets the source location of the qualifier. </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = QualifierAttributeMetadata.Location;
    
    /// <summary> Returns a string representation of the custom qualifier. </summary>
    /// <returns> A string in the format [@QualifierType]. </returns>
    public override string ToString() {
        return $"[@{QualifierAttributeMetadata.QualifierType}]";        
    }

    /// <summary> Compares this qualifier with another for equality. </summary>
    /// <param name="other"> The other qualifier to compare with. </param>
    /// <returns> True if the qualifiers are equal, false otherwise. </returns>
    public virtual bool Equals(IQualifierMetadata? other) {
        if (other is not CustomQualifierMetadata o) return false;
        if (ReferenceEquals(this, o)) return true;
        return Location.Equals(o.Location) && QualifierAttributeMetadata.Equals(o.QualifierAttributeMetadata);
    }

    public override int GetHashCode() {
        unchecked {
            return (Location.GetHashCode() * 397) ^ QualifierAttributeMetadata.GetHashCode();
        }
    }
}
