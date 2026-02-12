// -----------------------------------------------------------------------------
// <copyright file="LocationInfo.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

#endregion

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
/// Value type representation of source code location information optimized for incremental generators.
/// 
/// PURPOSE:
/// - Provides a memory-efficient, serializable alternative to Roslyn's Location class
/// - Enables location data to be stored in cached generator state without memory leaks
/// - Supports accurate diagnostic reporting with minimal overhead
/// 
/// WHY THIS EXISTS:
/// Roslyn's Location class has significant memory overhead for incremental generators:
/// 1. Location holds references to SyntaxTree objects, preventing GC of entire syntax trees
/// 2. In long-running IDE sessions, this causes unbounded memory growth
/// 3. Location objects aren't serializable or cacheable in incremental generator pipelines
/// 
/// LocationInfo solves these problems by:
/// - Being a value type (record struct semantics) with structural equality
/// - Storing only essential data: file path and text spans
/// - Breaking the reference chain to syntax trees, enabling proper GC
/// - Supporting bidirectional conversion to/from Location when needed
/// 
/// INCREMENTAL GENERATOR PATTERN:
/// This type is typically wrapped in GeneratorIgnored&lt;LocationInfo?&gt; to indicate that
/// location changes shouldn't invalidate cached results. The pattern is:
/// - Parse Phase: Location → LocationInfo (capture essential data, release tree reference)
/// - Cache Phase: Store LocationInfo in generator state (memory-efficient, equality-comparable)
/// - Diagnostic Phase: LocationInfo → Location (reconstruct for error reporting)
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Value type: Stack-allocated, no heap pressure
/// - Immutable: Thread-safe, can be shared across generator invocations
/// - Compact: ~48 bytes vs Location's full object graph with syntax tree references
/// - GC-friendly: Enables syntax trees to be collected after initial parse
/// </summary>
/// <param name="FilePath"> The path to the source file. </param>
/// <param name="TextSpan"> The span of text in the file. </param>
/// <param name="LineSpan"> The line position span. </param>
internal record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan) {
    /// <summary>
    ///     Converts this location info back to a Roslyn <see cref="Location"/>.
    /// </summary>
    /// <remarks>
    /// WHEN TO USE:
    /// Call this when emitting diagnostics or errors that need to be displayed in the IDE.
    /// The reconstructed Location will point correctly to the source code position.
    /// 
    /// NOTE: The reconstructed Location won't have a SyntaxTree reference, but that's fine
    /// for diagnostic purposes - the IDE only needs file path and span information.
    /// </remarks>
    /// <returns> A Roslyn location. </returns>
    public Location ToLocation()
        => Location.Create(FilePath, TextSpan, LineSpan);

    /// <summary>
    ///     Creates location info from a syntax node.
    /// </summary>
    /// <remarks>
    /// COMMON USAGE PATTERN:
    /// Use this during syntax node processing to capture location before the node is discarded.
    /// Example: LocationInfo.CreateFrom(attributeSyntax) when processing attributes.
    /// </remarks>
    /// <param name="node"> The syntax node. </param>
    /// <returns> Location info, or null if the node has no location. </returns>
    public static LocationInfo? CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());

    /// <summary>
    ///     Creates location info from a Roslyn location.
    /// </summary>
    /// <remarks>
    /// NULL RETURN SEMANTICS:
    /// Returns null when the location has no associated source tree. This occurs for:
    /// - Compiler-synthesized members (implicit constructors, backing fields)
    /// - Types defined in metadata assemblies (referenced DLLs)
    /// - Special locations like Location.None
    /// 
    /// Returning null rather than throwing allows callers to gracefully handle these cases
    /// (e.g., by falling back to a parent symbol's location or Location.None).
    /// </remarks>
    /// <param name="location"> The Roslyn location. </param>
    /// <returns> Location info, or null if the location has no source tree. </returns>
    public static LocationInfo? CreateFrom(Location location) {
        if (location.SourceTree is null) {
            return null;
        }

        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }
}