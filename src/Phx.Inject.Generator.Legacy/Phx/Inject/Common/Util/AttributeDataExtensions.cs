// -----------------------------------------------------------------------------
// <copyright file="AttributeDataExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common.Util;

internal static class AttributeDataExtensions {
    public static INamedTypeSymbol GetNamedTypeSymbol(this AttributeData attributeData) {
        return attributeData.AttributeClass
            ?? throw new InvalidOperationException("AttributeData does not have a valid AttributeClass.");
    }

    public static string GetFullyQualifiedName(this AttributeData attributeData) {
        return attributeData.AttributeClass?.ToString()
            ?? throw new InvalidOperationException("AttributeData does not have a valid AttributeClass.");
    }

    public static Location GetAttributeLocation(this AttributeData attributeData, ISymbol attributedSymbol) {
        return attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
            ?? attributedSymbol.GetLocationOrDefault();
    }
}
