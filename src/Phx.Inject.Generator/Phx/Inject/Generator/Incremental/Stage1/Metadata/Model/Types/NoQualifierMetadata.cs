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
///     Singleton representing an unqualified dependency binding with no distinguishing annotation.
/// </summary>
/// <remarks>
///     <para><b>Design Pattern - Null Object:</b></para>
///     <para>
///     Rather than using null to represent "no qualifier," we use this singleton to avoid null checks
///     throughout the codebase. This follows the Null Object pattern, allowing qualifier-handling
///     code to treat all cases uniformly without special-casing absence.
///     </para>
///     
///     <para><b>Singleton Rationale:</b></para>
///     <para>
///     Since all instances of "no qualifier" are semantically identical, we use a singleton to
///     reduce allocation pressure and enable reference equality checks (obj == NoQualifierMetadata.Instance)
///     as a fast path before falling back to structural equality.
///     </para>
///     
///     <para><b>Equality Semantics:</b></para>
///     <para>
///     All NoQualifierMetadata instances are equal by definition, regardless of whether they're
///     the singleton or a separate instance (though separate instances should never be created
///     due to the private constructor). This ensures consistent behavior even if deserialization
///     or reflection somehow produces additional instances.
///     </para>
///     
///     <para><b>Usage in Qualified Types:</b></para>
///     <para>
///     When paired with a TypeMetadata in QualifiedTypeMetadata, indicates this is the "default"
///     binding for the type with no special distinguisher. Injection sites without qualifiers
///     will match only bindings with NoQualifierMetadata (not labeled or custom-qualified bindings).
///     </para>
/// </remarks>
internal record NoQualifierMetadata : IQualifierMetadata {
    /// <summary>
    ///     Gets the singleton instance representing the absence of a qualifier.
    /// </summary>
    /// <remarks>
    ///     Always use this instance rather than constructing new instances. The private constructor
    ///     enforces this pattern to maintain the singleton guarantee.
    /// </remarks>
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
    /// <param name="other">The qualifier to compare against, or null.</param>
    /// <returns>
    ///     True if other is also a NoQualifierMetadata (any instance), false for any other
    ///     qualifier type or null.
    /// </returns>
    /// <remarks>
    ///     Uses type checking rather than reference equality to handle the (unlikely) case
    ///     where reflection or deserialization creates non-singleton instances.
    /// </remarks>
    public virtual bool Equals(IQualifierMetadata? other) {
        return other is NoQualifierMetadata;
    }

    /// <summary>
    ///     Returns a stable hash code for the "no qualifier" state.
    /// </summary>
    /// <returns>
    ///     A hash code that's identical for all NoQualifierMetadata instances, ensuring
    ///     consistent hashing behavior in dictionaries and hash sets.
    /// </returns>
    public override int GetHashCode() {
        return Location.GetHashCode();
    }
}
