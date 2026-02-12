// -----------------------------------------------------------------------------
// <copyright file="LabelQualifierMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

internal record LabelQualifierMetadata(
    LabelAttributeMetadata LabelAttributeMetadata
) : IQualifierMetadata {
    public GeneratorIgnored<LocationInfo?> Location { get; } = LabelAttributeMetadata.Location;
    
    public override string ToString() {
        return $"[Label({LabelAttributeMetadata.Label})]";        
    }

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
