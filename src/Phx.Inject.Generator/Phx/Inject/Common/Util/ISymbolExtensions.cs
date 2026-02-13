// -----------------------------------------------------------------------------
// <copyright file="ISymbolExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Common.Util;

/// <summary>
/// Extension methods for working with Roslyn <see cref="ISymbol"/> types.
/// 
/// PURPOSE:
/// - Provides null-safe and consistent ways to extract common symbol information
/// - Abstracts Roslyn API quirks and verbosity
/// - Supports diagnostic reporting with sensible location fallbacks
/// 
/// WHY THIS EXISTS:
/// Roslyn's symbol APIs have several pain points:
/// 1. ToString() on symbols produces compiler-internal representations that differ from source code
/// 2. Symbols can be null in error scenarios (unresolved types, malformed code)
/// 3. Location handling is inconsistent (symbols may have 0, 1, or many locations)
/// 
/// These extensions provide:
/// - Consistent fully-qualified name formatting across all symbol types
/// - Graceful null handling with explicit "[null]" markers
/// - Location resolution with sensible fallbacks for diagnostics
/// 
/// ROSLYN INTEGRATION:
/// 
/// ISymbol.ToString() behavior:
/// - For types: Produces fully-qualified names like "System.Collections.Generic.List<T>"
/// - For members: Includes declaring type like "MyClass.MyMethod()"
/// - For error types: May return special markers or compiler-internal names
/// 
/// These extensions standardize the output format and make null handling explicit.
/// 
/// COMMON PATTERNS:
/// 
/// 1. Type name extraction:
///    var typeName = typeSymbol.GetFullyQualifiedName();
///    // "System.Collections.Generic.List<System.String>"
/// 
/// 2. Base name without generics:
///    var baseName = typeSymbol.GetFullyQualifiedBaseName();
///    // "System.Collections.Generic.List"
/// 
/// 3. Location for diagnostics:
///    var location = symbol.GetLocationOrDefault();
///    // Symbol's first location, or Location.None if unavailable
/// 
/// DESIGN DECISIONS:
/// 
/// 1. Why return "[null]" instead of throwing?
///    - Null symbols occur in malformed or partially-compiled code
///    - Generators should surface diagnostics rather than crashing
///    - "[null]" makes it obvious in error messages what went wrong
/// 
/// 2. Why GetFullyQualifiedBaseName?
///    - Sometimes you need to identify a generic type without its arguments
///    - Example: Check if a type is List&lt;T&gt; regardless of T
///    - ContainingNamespace + Name gives you "System.Collections.Generic.List"
/// 
/// 3. Why FirstOrDefault for location?
///    - Symbols can have multiple locations (partial classes, linked files)
///    - First location is typically the "primary" declaration
///    - Fallback ensures diagnostics always have somewhere to point
/// </summary>
internal static class ISymbolExtensions {
    /// <summary>
    ///     Gets the fully-qualified name of a type symbol.
    /// </summary>
    /// <remarks>
    /// DELEGATES TO TOSTRING:
    /// Uses Roslyn's ITypeSymbol.ToString() which produces fully-qualified names with generic
    /// parameters. For example: "System.Collections.Generic.Dictionary&lt;System.String, System.Int32&gt;"
    /// 
    /// NULL HANDLING:
    /// Returns "[null]" for null symbols rather than throwing. This is useful for:
    /// - Diagnostic messages that need to show what type was expected
    /// - Error scenarios where types couldn't be resolved
    /// - Defensive coding in semantic analysis
    /// </remarks>
    /// <param name="symbol"> The type symbol. </param>
    /// <returns> The fully-qualified type name, or "[null]" if the symbol is null. </returns>
    public static string GetFullyQualifiedName(this ITypeSymbol? symbol) {
        return symbol == null
            ? "[null]"
            : symbol.ToString();
    }
    
    /// <summary>
    ///     Gets the fully-qualified base name of a type symbol without generic parameters.
    /// </summary>
    /// <remarks>
    /// USE CASE:
    /// When you need to identify a generic type definition without caring about its type arguments.
    /// Example: Checking if a type is List&lt;T&gt; or Dictionary&lt;K,V&gt; regardless of T, K, V.
    /// 
    /// COMPARISON:
    /// - GetFullyQualifiedName(): "System.Collections.Generic.List&lt;System.String&gt;"
    /// - GetFullyQualifiedBaseName(): "System.Collections.Generic.List"
    /// 
    /// IMPLEMENTATION:
    /// Concatenates ContainingNamespace + Name, which gives the type identity without parameters.
    /// </remarks>
    /// <param name="symbol"> The type symbol. </param>
    /// <returns> The fully-qualified base name, or "[null]" if the symbol is null. </returns>
    public static string GetFullyQualifiedBaseName(this ITypeSymbol? symbol) {
        return symbol == null
            ? "[null]"
            : $"{symbol.ContainingNamespace}.{symbol.Name}";
    }

    /// <summary>
    ///     Gets the source location of a symbol, or a default location if not available.
    /// </summary>
    /// <remarks>
    /// FALLBACK CHAIN:
    /// 1. First location from symbol.Locations (typically the primary declaration)
    /// 2. Provided defaultLocation parameter (allows context-specific fallback)
    /// 3. Location.None (ensures non-null return)
    /// 
    /// MULTIPLE LOCATIONS:
    /// Symbols can have multiple locations due to:
    /// - Partial classes/methods across files
    /// - Source link or metadata references
    /// - Implicit implementations
    /// 
    /// FirstOrDefault() picks the first location, which is usually the most relevant
    /// (Roslyn typically orders with primary declaration first).
    /// 
    /// USE IN DIAGNOSTICS:
    /// Ensures every diagnostic has a valid location to display, even for:
    /// - Metadata-only types (referenced assemblies)
    /// - Compiler-synthesized members
    /// - Types from error recovery
    /// </remarks>
    /// <param name="symbol"> The symbol. </param>
    /// <param name="defaultLocation"> The default location to use if the symbol has no location. </param>
    /// <returns> The symbol's location, or the default location, or <see cref="Location.None"/>. </returns>
    public static Location GetLocationOrDefault(this ISymbol? symbol, Location? defaultLocation = null) {
        return symbol?.Locations.FirstOrDefault() ?? defaultLocation ?? Location.None;
    }
}
