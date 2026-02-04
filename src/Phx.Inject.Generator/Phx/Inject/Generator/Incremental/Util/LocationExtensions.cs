// -----------------------------------------------------------------------------
// <copyright file="SourceLocation.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

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
    ///     Returns the <see cref="Location"/> or <see cref="Location.None"/> if the value is <c> null </c>.
    /// </summary>
    /// <param name="location"> The location to return. </param>
    /// <returns> An instance of <see cref="Location"/>. </returns>
    public static GeneratorIgnored<Location> OrNone(this GeneratorIgnored<Location>? location) {
        return location ?? new GeneratorIgnored<Location>(Location.None);
    }
}
