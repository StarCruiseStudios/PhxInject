// -----------------------------------------------------------------------------
// <copyright file="GeneratorIgnored.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
///     Wraps a type in a way that only checks type identity for equality comparisons, allowing
///     a value to be used in incremental generator models without triggering regeneration when the
///     value changes.
/// </summary>
/// <param name="value"> The value referenced. </param>
public class GeneratorIgnored<T>(T value) {
    /// <summary> The wrapped value. </summary>
    public T Value { get; } = value;
    
    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public virtual bool Equals(GeneratorIgnored<T>? other) {
        return other is not null;
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode() {
        return typeof(T).GetHashCode();
    }
    
    /// <inheritdoc cref="Object.ToString"/>
    public override string ToString() {
        return Value?.ToString() ?? "null";
    }
}

/// <summary>
///     Extension methods for creating <see cref="GeneratorIgnored{T}"/> instances.
/// </summary>
public static class GeneratorIgnoredExtensions {
    /// <summary>
    ///     Wraps a value in a <see cref="GeneratorIgnored{T}"/> wrapper.
    /// </summary>
    /// <typeparam name="T"> The type of the value. </typeparam>
    /// <param name="value"> The value to wrap. </param>
    /// <returns> A wrapped value that ignores equality comparisons. </returns>
    public static GeneratorIgnored<T> GeneratorIgnored<T>(this T value) {
        return new GeneratorIgnored<T>(value);
    }
}