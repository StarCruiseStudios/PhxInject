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

public static class LocationExtensions {
    /// <summary>
    ///     Returns the <see cref="Location"/> or <see cref="Location.None"/> if the value is <c> null </c>.
    /// </summary>
    /// <param name="location"> The location to return. </param>
    /// <returns> An instance of <see cref="Location"/>. </returns>
    public static Location OrNone(this Location? location) {
        return location ?? Location.None;
    }
    
    /// <summary>
    ///     Wraps a <see cref="Location"/> in a <see cref="GeneratorIgnored{T}"/> wrapper.
    /// </summary>
    /// <param name="location"> The location to wrap. </param>
    /// <returns> A <see cref="GeneratorIgnored{LocationInfo}"/> wrapper. </returns>
    internal static GeneratorIgnored<LocationInfo?> GeneratorIgnored(this Location? location) {
        return new GeneratorIgnored<LocationInfo?>(LocationInfo.CreateFrom(location.OrNone()));
    }
    
    /// <summary>
    ///     Returns the <see cref="GeneratorIgnored{LocationInfo}"/> or a default value if the input is <c> null </c>.
    /// </summary>
    /// <param name="location"> The location to return. </param>
    /// <returns> An instance of <see cref="GeneratorIgnored{LocationInfo}"/>. </returns>
    internal static GeneratorIgnored<LocationInfo?> OrNone(this GeneratorIgnored<LocationInfo?>? location) {
        return location ?? new GeneratorIgnored<LocationInfo?>(null);
    }
}
