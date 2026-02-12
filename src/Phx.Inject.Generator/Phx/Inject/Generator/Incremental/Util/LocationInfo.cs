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
///     Represents source code location information that can be used in incremental generators.
/// </summary>
/// <param name="FilePath"> The path to the source file. </param>
/// <param name="TextSpan"> The span of text in the file. </param>
/// <param name="LineSpan"> The line position span. </param>
internal record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan) {
    /// <summary>
    ///     Converts this location info back to a Roslyn <see cref="Location"/>.
    /// </summary>
    /// <returns> A Roslyn location. </returns>
    public Location ToLocation()
        => Location.Create(FilePath, TextSpan, LineSpan);

    /// <summary>
    ///     Creates location info from a syntax node.
    /// </summary>
    /// <param name="node"> The syntax node. </param>
    /// <returns> Location info, or null if the node has no location. </returns>
    public static LocationInfo? CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());

    /// <summary>
    ///     Creates location info from a Roslyn location.
    /// </summary>
    /// <param name="location"> The Roslyn location. </param>
    /// <returns> Location info, or null if the location has no source tree. </returns>
    public static LocationInfo? CreateFrom(Location location) {
        if (location.SourceTree is null) {
            return null;
        }

        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }
}