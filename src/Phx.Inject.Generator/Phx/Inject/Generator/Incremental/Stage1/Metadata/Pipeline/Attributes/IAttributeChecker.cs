// -----------------------------------------------------------------------------
// <copyright file="IAttributeChecker.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Checks if a symbol has a specific attribute.
/// </summary>
/// <remarks>
///     <para><b>Roslyn Symbol Attribute Query Pattern:</b></para>
///     <para>
///     Provides efficient existence checks for attributes on Roslyn symbols without materializing
///     full attribute metadata. This is the foundation of the attribute transformer pipeline's
///     conditional execution strategy.
///     </para>
///     
///     <para><b>Performance Optimization - Why Separate Existence Check:</b></para>
///     <para>
///     Checking attribute existence (HasAttribute) is significantly cheaper than transforming
///     attribute data into metadata (Transform). Roslyn's GetAttributes() returns AttributeData
///     objects that require walking constructor arguments, named arguments, and performing type
///     resolution. By separating the check, we can:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Skip expensive transformation when attribute is absent (common case for most symbols)
///             </description>
///         </item>
///         <item>
///             <description>
///             Support conditional transformation via TransformOrNull pattern (check-then-transform)
///             </description>
///         </item>
///         <item>
///             <description>
///             Enable fast filter operations in validation pipelines without allocating metadata objects
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Integration with Validators:</b></para>
///     <para>
///     ICodeElementValidator implementations use HasAttribute during IsValidSymbol checks to verify
///     required attributes exist before attempting transformation. This separates structural validation
///     (does attribute exist?) from semantic validation (is attribute data valid?), allowing validators
///     to fail fast and report more precise diagnostic messages.
///     </para>
///     
///     <para><b>Thread Safety:</b></para>
///     <para>
///     Implementations must be thread-safe. Roslyn's incremental generator pipeline calls into
///     transformers from parallel worker threads during batch processing. The underlying
///     ISymbol.GetAttributes() is thread-safe as Roslyn symbols are immutable.
///     </para>
/// </remarks>
internal interface IAttributeChecker {
    /// <summary>
    ///     Determines if the target symbol has the attribute.
    /// </summary>
    /// <param name="targetSymbol">The symbol to check.</param>
    /// <returns>True if the attribute is present; otherwise, false.</returns>
    /// <remarks>
    ///     <para>
    ///     Implementation typically calls ISymbol.GetAttributes() and checks for matching
    ///     AttributeClass fully qualified name. This is an O(n) scan where n is the number
    ///     of attributes on the symbol (typically small, &lt;5 in most code).
    ///     </para>
    /// </remarks>
    bool HasAttribute(ISymbol targetSymbol);
}
