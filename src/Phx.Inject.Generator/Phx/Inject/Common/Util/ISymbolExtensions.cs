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

internal static class ISymbolExtensions {
    public static string GetFullyQualifiedName(this ITypeSymbol? symbol) {
        return symbol == null
            ? "[null]"
            : symbol.ToString();
    }
    
    public static string GetFullyQualifiedBaseName(this ITypeSymbol? symbol) {
        return symbol == null
            ? "[null]"
            : $"{symbol.ContainingNamespace}.{symbol.Name}";
    }

    public static Location GetLocationOrDefault(this ISymbol? symbol, Location? defaultLocation = null) {
        return symbol?.Locations.FirstOrDefault() ?? defaultLocation ?? Location.None;
    }
}
