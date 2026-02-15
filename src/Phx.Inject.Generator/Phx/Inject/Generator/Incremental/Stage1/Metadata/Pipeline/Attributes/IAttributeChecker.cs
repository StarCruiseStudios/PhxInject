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
///     Efficient existence check without materializing attribute metadata. Cheaper than transformation
///     (skip when absent). Enables conditional transformation via <c>TransformOrNull</c> pattern.
///     Must be thread-safe (Roslyn incremental pipeline uses parallel processing).
/// </remarks>
internal interface IAttributeChecker {
    /// <summary>
    ///     Determines if the target symbol has the attribute.
    /// </summary>
    /// <param name="targetSymbol">The symbol to check.</param>
    /// <returns>True if the attribute is present; otherwise, false.</returns>
    bool HasAttribute(ISymbol targetSymbol);
}
