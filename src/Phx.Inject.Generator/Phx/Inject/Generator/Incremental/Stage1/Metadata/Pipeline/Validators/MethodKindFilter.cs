// -----------------------------------------------------------------------------
// <copyright file="MethodKindFilter.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

internal enum MethodKindFilter {
    Method,
    Constructor,
    Getter,
    Setter,
    Any
}

internal static class MethodKindFilterExtensions {
    internal static bool MethodKindMatches(this MethodKindFilter filter, IMethodSymbol methodSymbol) {
        return filter switch {
            MethodKindFilter.Method => methodSymbol.MethodKind == MethodKind.Ordinary,
            MethodKindFilter.Constructor => methodSymbol.MethodKind == MethodKind.Constructor,
            MethodKindFilter.Getter => methodSymbol.MethodKind == MethodKind.PropertyGet,
            MethodKindFilter.Setter => methodSymbol.MethodKind == MethodKind.PropertySet,
            MethodKindFilter.Any => true,
            _ => false // Unknown method kind filter, treat as no match
        };
    }
}
