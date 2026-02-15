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
///     Specifies accessibility requirements for code elements in dependency injection contexts.
/// </summary>
/// <remarks>
///     Defines accessibility constraints for code generation. <c>PublicOrInternal</c> requires
///     elements to be accessible to generated code. <c>Any</c> accepts all accessibility levels.
/// </remarks>
internal enum CodeElementAccessibility {
    /// <summary>
    ///     Requires the code element to be public or internal.
    /// </summary>
    /// <remarks>
    ///     This is the typical requirement for dependency injection framework elements.
    ///     Internal is acceptable because it remains accessible within the same assembly
    ///     or via [InternalsVisibleTo].
    /// </remarks>
    PublicOrInternal,
    
    /// <summary>
    ///     Allows the code element to have any accessibility level.
    /// </summary>
    /// <remarks>
    ///     Used when reading metadata or generating diagnostics regardless of accessibility.
    ///     For example, reporting all classes with [Injector] attribute even if they're private,
    ///     so diagnostics can explain why private injectors are not supported.
    /// </remarks>
    Any
}

internal static class CodeElementAccessibilityExtensions {
    /// <summary>
    ///     Checks if a Roslyn Accessibility value satisfies the required accessibility constraint.
    /// </summary>
    /// <param name="accessibility">The required accessibility constraint.</param>
    /// <param name="other">The actual accessibility of the code element being validated.</param>
    /// <returns>
    ///     <see langword="true" /> if the actual accessibility satisfies the requirement; 
    ///     otherwise, <see langword="false" />.
    /// </returns>
    internal static bool AccessibilityMatches(this CodeElementAccessibility accessibility, Accessibility other) {
        return accessibility switch {
            CodeElementAccessibility.PublicOrInternal => other is Accessibility.Public or Accessibility.Internal,
            CodeElementAccessibility.Any => true,
            _ => false // Unknown accessibility, treat as no match
        };
    }
}