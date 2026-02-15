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
///     Encodes accessibility constraints for successful code generation. <c>PublicOrInternal</c>
///     allows generated code to reference elements (protected/private prevent generation).
///     <c>Any</c> accepts all accessibility levels (diagnostic reporting on inaccessible elements).
///     Intentionally limited to generation needs, not matching Roslyn's full <c>Accessibility</c> enum.
/// </remarks>
internal enum CodeElementAccessibility {
    /// <summary>
    ///     Requires the code element to be public or internal (accessible to generated code).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This is the typical requirement for DI framework elements. Generated code must be able
    ///     to reference classes, call methods, and implement interfaces. Private or protected
    ///     accessibility would make generated code uncompilable or inaccessible at usage sites.
    ///     </para>
    ///     <para>
    ///     Internal is acceptable because generated code typically lives in the same assembly,
    ///     or uses [InternalsVisibleTo] to access internal members.
    ///     </para>
    /// </remarks>
    PublicOrInternal,
    
    /// <summary>
    ///     Allows the code element to have any accessibility level.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Used when reading metadata or generating diagnostics regardless of accessibility.
    ///     For example, reporting all classes with @Injector attribute even if they're private
    ///     (so we can emit a diagnostic explaining why private injectors don't work).
    ///     </para>
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
    ///     True if the actual accessibility satisfies the requirement, false otherwise.
    /// </returns>
    /// <remarks>
    ///     <para>Protected Accessibility Rejection:</para>
    ///     <para>
    ///     Protected and ProtectedAndInternal are explicitly rejected for PublicOrInternal requirement.
    ///     Generated code doesn't inherit from user classes, so protected members are inaccessible
    ///     even though they're not strictly "private". This prevents confusing scenarios where
    ///     protected factory methods compile but cannot be called by generated injectors.
    ///     </para>
    ///     
    ///     <para>ProtectedOrInternal Acceptance:</para>
    ///     <para>
    ///     ProtectedOrInternal (aka 'protected internal') allows access from same assembly, which
    ///     satisfies our PublicOrInternal requirement. The 'protected' part is irrelevant since
    ///     we don't inherit, but the 'internal' part makes it accessible.
    ///     </para>
    /// </remarks>
    internal static bool AccessibilityMatches(this CodeElementAccessibility accessibility, Accessibility other) {
        return accessibility switch {
            CodeElementAccessibility.PublicOrInternal => other is Accessibility.Public or Accessibility.Internal,
            CodeElementAccessibility.Any => true,
            _ => false // Unknown accessibility, treat as no match
        };
    }
}