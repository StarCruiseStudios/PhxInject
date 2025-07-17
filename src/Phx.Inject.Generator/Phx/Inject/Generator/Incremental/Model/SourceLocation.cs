// -----------------------------------------------------------------------------
// <copyright file="SourceLocation.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Incremental.Model;

/// <summary>
///     Wrap the <see cref="Location"/> type to provide a more convenient way to handle comparison
///     requirements for the source generator.
/// </summary>
/// <param name="Location"> The source location referenced by <see cref="ISourceCodeElement"/>. </param>
public record SourceLocation(Location Location) {
    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public virtual bool Equals(SourceLocation? other) {
        if (other is null) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        var thisSpan = Location.GetLineSpan();
        var otherSpan = other.Location.GetLineSpan();
        return thisSpan.Span.Equals(otherSpan.Span)
            && thisSpan.Path.Equals(otherSpan.Path);
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode() {
        var span = Location.GetLineSpan();
        var hash = 17;
        hash = hash * 31 + span.Span.GetHashCode();
        hash = hash * 31 + span.Path.GetHashCode();
        return hash;
    }

    /// <inheritdoc cref="Object.ToString"/>
    public override string ToString() {
        return Location.ToString();
    }
}

public static class SourceLocationExtensions {
    /// <summary> Converts a <see cref="Location"/> to a <see cref="SourceLocation"/>. </summary>
    /// <param name="location"> The location to convert. </param>
    /// <returns> A new instance of <see cref="SourceLocation"/>. </returns>
    public static SourceLocation ToSourceLocation(this Location? location) {
        return new SourceLocation(location ?? Location.None);
    }

    /// <summary>
    ///     Returns the <see cref="SourceLocation"/> or the default <see cref="SourceLocation"/> if the
    ///     value is <c> null </c>.
    /// </summary>
    /// <param name="sourceLocation"> The location to convert. </param>
    /// <returns> An instance of <see cref="SourceLocation"/>. </returns>
    public static SourceLocation OrDefault(this SourceLocation? sourceLocation) {
        return sourceLocation ?? new SourceLocation(Location.None);
    }
}
