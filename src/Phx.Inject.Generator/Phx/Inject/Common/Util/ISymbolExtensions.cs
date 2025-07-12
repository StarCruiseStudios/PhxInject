// -----------------------------------------------------------------------------
// <copyright file="ITypeSymbolExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common.Util;

internal static class ISymbolExtensions {
    public static string GetFullyQualifiedName(this ITypeSymbol? symbol) {
        return symbol == null
            ? "[null]"
            : symbol.ToString();
    }

    public static Location GetLocationOrDefault(this ISymbol? symbol, Location? defaultLocation = null) {
        return symbol?.Locations.FirstOrDefault() ?? defaultLocation ?? Location.None;
    }
}
