// -----------------------------------------------------------------------------
// <copyright file="GeneratorIgnored.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
///     Type wrapper that excludes a value from incremental generator equality comparisons,
///     preventing unnecessary regeneration when the wrapped value changes.
/// </summary>
/// <typeparam name="T">
///     The type of value being wrapped. Typically non-semantic data like source locations.
/// </typeparam>
/// <param name="value">
///     The value to wrap. Its changes will not trigger incremental regeneration.
/// </param>
/// <remarks>
///     <para><b>Purpose:</b></para>
///     <para>
///     Roslyn's incremental generators use value equality on model objects to determine if
///     regeneration is needed. However, some data (like source file locations) is essential
///     for diagnostics but irrelevant for code generation decisions. Wrapping such values
///     prevents spurious regeneration when, for example, code is reformatted without changing
///     semantics.
///     </para>
///     
///     <para><b>Equality Semantics:</b></para>
///     <para>
///     Two <c>GeneratorIgnored&lt;T&gt;</c> instances are always considered equal regardless
///     of their wrapped values, as long as they wrap the same type <c>T</c>. This intentionally
///     violates normal equality contracts but is correct in the context of incremental
///     compilation where we want to signal "this data doesn't affect output."
///     </para>
///     
///     <para><b>Usage Pattern:</b></para>
///     <para>
///     Primarily used for <see cref="LocationInfo"/> fields in metadata records. Location
///     data is needed to report diagnostics at the correct source positions but shouldn't
///     trigger recompilation if a type definition is moved within a file.
///     </para>
///     
///     <para><b>Performance:</b></para>
///     <para>
///     Minimal overhead - just a single object allocation and type-based hash code.
///     Avoids expensive deep comparisons of location data during incremental compilation checks.
///     </para>
///     
///     <para><b>When NOT to use:</b></para>
///     <para>
///     Do not wrap semantic data that affects code generation. If changing the value should
///     trigger regeneration, do not use this wrapper.
///     </para>
/// </remarks>
public class GeneratorIgnored<T>(T value) {
    /// <summary>
    ///     Gets the wrapped value.
    /// </summary>
    /// <remarks>
    ///     Access this when you need the actual data (e.g., reporting a diagnostic).
    ///     Do not use this value in equality comparisons that affect incremental compilation.
    /// </remarks>
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
///     Extension methods for wrapping values in <see cref="GeneratorIgnored{T}"/> instances.
/// </summary>
/// <remarks>
///     Provides convenient fluent syntax for creating ignored wrappers, typically used
///     when constructing metadata records from Roslyn symbols.
/// </remarks>
public static class GeneratorIgnoredExtensions {
    /// <summary>
    ///     Wraps a value so it won't affect incremental generator equality comparisons.
    /// </summary>
    /// <typeparam name="T">The type of value to wrap.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>
    ///     A wrapped value that appears equal to all other <c>GeneratorIgnored&lt;T&gt;</c>
    ///     instances regardless of the wrapped value.
    /// </returns>
    /// <example>
    ///     <code>
    ///     var location = symbol.Locations.First().GeneratorIgnored();
    ///     // location changes won't trigger regeneration
    ///     </code>
    /// </example>
    public static GeneratorIgnored<T> GeneratorIgnored<T>(this T value) {
        return new GeneratorIgnored<T>(value);
    }
}