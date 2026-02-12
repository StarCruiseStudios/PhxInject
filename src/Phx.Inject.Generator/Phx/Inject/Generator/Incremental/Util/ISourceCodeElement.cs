// -----------------------------------------------------------------------------
// <copyright file="ISourceCodeElement.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
///     Represents a source code element that has an associated location.
/// </summary>
internal interface ISourceCodeElement {
    /// <summary> Gets the source location of the element. </summary>
    GeneratorIgnored<LocationInfo?> Location { get; }
}
