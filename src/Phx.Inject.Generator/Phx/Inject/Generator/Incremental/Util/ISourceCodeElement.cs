// -----------------------------------------------------------------------------
// <copyright file="ISourceCodeElement.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
/// Marker interface for source code elements that have an associated location.
/// 
/// PURPOSE:
/// - Provides a common contract for all parsed metadata objects to carry location information
/// - Enables generic diagnostic reporting without knowing the specific element type
/// - Supports location-aware error messages throughout the generator pipeline
/// 
/// WHY THIS EXISTS:
/// In a source generator, nearly every semantic element (types, methods, attributes, etc.)
/// needs location tracking for diagnostics. This interface:
/// 1. Establishes a consistent pattern across all metadata models
/// 2. Enables polymorphic diagnostic helpers that work on any source element
/// 3. Documents the design requirement that metadata must preserve source locations
/// 
/// DESIGN DECISION - GeneratorIgnored wrapper:
/// The Location property uses GeneratorIgnored&lt;LocationInfo?&gt; rather than LocationInfo? because:
/// - Locations are needed for diagnostics but shouldn't affect equality/caching
/// - If two elements differ only in their location (e.g., due to code reformatting),
///   they should still be considered equal by the incremental generator
/// - This prevents unnecessary regeneration when files are reformatted or moved
/// 
/// USAGE PATTERN:
/// Implement this interface on all metadata records that represent user-written code:
///   record TypeMetadata(...) : ISourceCodeElement {
///       public GeneratorIgnored&lt;LocationInfo?&gt; Location { get; init; }
///   }
/// 
/// Then use for diagnostics:
///   void ReportError(ISourceCodeElement element, string message) {
///       var location = element.Location.Value?.ToLocation() ?? Location.None;
///       context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));
///   }
/// </summary>
internal interface ISourceCodeElement {
    /// <summary> Gets the source location of the element. </summary>
    GeneratorIgnored<LocationInfo?> Location { get; }
}
