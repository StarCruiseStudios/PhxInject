// -----------------------------------------------------------------------------
// <copyright file="CodeElementAccessibility.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

/// <summary>
///     Specifies accessibility requirements for code elements.
/// </summary>
internal enum CodeElementAccessibility {
    /// <summary>
    ///     Requires the code element to be public or internal.
    /// </summary>
    PublicOrInternal,
    /// <summary>
    ///     Allows the code element to have any accessibility.
    /// </summary>
    Any
}

internal static class CodeElementAccessibilityExtensions {
    internal static bool AccessibilityMatches(this CodeElementAccessibility accessibility, Accessibility other) {
        return accessibility switch {
            CodeElementAccessibility.PublicOrInternal => other is Accessibility.Public or Accessibility.Internal,
            CodeElementAccessibility.Any => true,
            _ => false // Unknown accessibility, treat as no match
        };
    }
}