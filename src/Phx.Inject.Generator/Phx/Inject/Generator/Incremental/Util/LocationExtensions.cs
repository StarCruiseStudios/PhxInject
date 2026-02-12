// -----------------------------------------------------------------------------
// <copyright file="LocationExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
/// Extension methods for Roslyn <see cref="Location"/> objects to support incremental generator patterns.
/// 
/// PURPOSE:
/// - Provides null-safe operations for locations throughout the generator pipeline
/// - Integrates Location instances with the incremental generator's caching system via GeneratorIgnored
/// - Ensures diagnostic messages have valid locations even when source information is unavailable
/// 
/// WHY THIS EXISTS:
/// Roslyn's Location can be null in various scenarios (generated code, compiler-synthesized members, metadata).
/// In incremental generators, proper location handling is critical for:
/// 1. Accurate diagnostic reporting that points users to the exact source of issues
/// 2. Correct caching behavior - locations affect equality comparisons but shouldn't trigger regeneration
/// 3. Safe downstream code that doesn't need constant null checks
/// 
/// INCREMENTAL GENERATOR INTEGRATION:
/// The GeneratorIgnored wrapper tells the incremental generator that Location changes should NOT
/// trigger regeneration. This is essential because:
/// - Location changes (file moves, line number shifts) don't affect semantic meaning
/// - Without this, any code reformatting would invalidate the entire cache
/// - Diagnostics still get accurate locations, but they don't participate in equality checks
/// </summary>
public static class LocationExtensions {
    /// <summary>
    ///     Returns the <see cref="Location"/> or <see cref="Location.None"/> if the value is <c> null </c>.
    /// </summary>
    /// <remarks>
    /// PATTERN: Null-coalescing for safe location handling.
    /// Use this at API boundaries where Location? needs to become Location for downstream consumers.
    /// Location.None is a special sentinel that indicates "no source location" rather than null.
    /// </remarks>
    /// <param name="location"> The location to return. </param>
    /// <returns> An instance of <see cref="Location"/>. </returns>
    public static Location OrNone(this Location? location) {
        return location ?? Location.None;
    }
    
    /// <summary>
    ///     Wraps a <see cref="Location"/> in a <see cref="GeneratorIgnored{T}"/> wrapper.
    /// </summary>
    /// <remarks>
    /// CRITICAL FOR INCREMENTAL PERFORMANCE:
    /// This wraps location data so it's preserved for diagnostics but excluded from equality checks.
    /// Without this, minor source changes (whitespace, comments, line shifts) would invalidate
    /// the incremental cache and force full regeneration.
    /// 
    /// DESIGN DECISION:
    /// We convert to LocationInfo rather than storing Location directly because:
    /// - Location objects hold references to syntax trees, preventing GC
    /// - LocationInfo is a value type with only the essential data (file path, spans)
    /// - This reduces memory pressure in long-running IDE scenarios
    /// </remarks>
    /// <param name="location"> The location to wrap. </param>
    /// <returns> A <see cref="GeneratorIgnored{LocationInfo}"/> wrapper. </returns>
    internal static GeneratorIgnored<LocationInfo?> GeneratorIgnored(this Location? location) {
        return new GeneratorIgnored<LocationInfo?>(LocationInfo.CreateFrom(location.OrNone()));
    }
    
    /// <summary>
    ///     Returns the <see cref="GeneratorIgnored{LocationInfo}"/> or a default value if the input is <c> null </c>.
    /// </summary>
    /// <remarks>
    /// PATTERN: Defensive null-handling for optional location wrappers.
    /// Common in scenarios where location tracking is conditional or may not be initialized.
    /// </remarks>
    /// <param name="location"> The location to return. </param>
    /// <returns> An instance of <see cref="GeneratorIgnored{LocationInfo}"/>. </returns>
    internal static GeneratorIgnored<LocationInfo?> OrNone(this GeneratorIgnored<LocationInfo?>? location) {
        return location ?? new GeneratorIgnored<LocationInfo?>(null);
    }
}
