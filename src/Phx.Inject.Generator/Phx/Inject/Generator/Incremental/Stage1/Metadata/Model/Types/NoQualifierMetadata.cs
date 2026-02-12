// -----------------------------------------------------------------------------
// <copyright file="NoQualifierMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing the absence of a qualifier annotation.
/// </summary>
internal record NoQualifierMetadata : IQualifierMetadata {
    /// <summary> Gets the singleton instance of NoQualifierMetadata. </summary>
    public static NoQualifierMetadata Instance { get; } = new();
    
    /// <summary> Gets the source location (always null for no qualifier). </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = new(null);
    
    private NoQualifierMetadata() { }

    /// <summary> Compares this no-qualifier with another for equality. </summary>
    /// <param name="other"> The other qualifier to compare with. </param>
    /// <returns> True if the other is also a NoQualifierMetadata, false otherwise. </returns>
    public virtual bool Equals(IQualifierMetadata? other) {
        return other is NoQualifierMetadata;
    }

    public override int GetHashCode() {
        return Location.GetHashCode();
    }
}
