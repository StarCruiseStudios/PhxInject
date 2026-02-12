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
///     Extension methods for working with Roslyn <see cref="ISymbol"/> types.
/// </summary>
internal static class ISymbolExtensions {
    /// <summary>
    ///     Gets the fully-qualified name of a type symbol.
    /// </summary>
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
    /// <param name="symbol"> The symbol. </param>
    /// <param name="defaultLocation"> The default location to use if the symbol has no location. </param>
    /// <returns> The symbol's location, or the default location, or <see cref="Location.None"/>. </returns>
    public static Location GetLocationOrDefault(this ISymbol? symbol, Location? defaultLocation = null) {
        return symbol?.Locations.FirstOrDefault() ?? defaultLocation ?? Location.None;
    }
}
