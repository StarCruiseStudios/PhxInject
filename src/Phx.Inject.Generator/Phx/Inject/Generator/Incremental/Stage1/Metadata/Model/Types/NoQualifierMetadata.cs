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
///     Singleton representing an unqualified dependency binding (Null Object pattern).
/// </summary>
/// <remarks>
///     Represents the absence of a qualifier annotation on a dependency. Uses the singleton pattern
///     to avoid null checks and enable fast reference equality. When paired with a type in
///     <see cref="QualifiedTypeMetadata"/>, indicates the default binding with no distinguisher.
/// </remarks>
internal sealed record NoQualifierMetadata : IQualifierMetadata {
    /// <summary>
    ///     Gets the singleton instance representing the absence of a qualifier.
    /// </summary>
    public static NoQualifierMetadata Instance { get; } = new();
    
    /// <summary>
    ///     Gets the source location, always null since "no qualifier" has no source representation.
    /// </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = new(null);
    
    /// <summary>
    ///     Private constructor enforces singleton pattern.
    /// </summary>
    private NoQualifierMetadata() { }

    /// <summary>
    ///     Determines whether another qualifier represents the same "no qualifier" state.
    /// </summary>
    public bool Equals(IQualifierMetadata? other) {
        return other is NoQualifierMetadata;
    }

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode() {
        return typeof(NoQualifierMetadata).GetHashCode();
    }
}
