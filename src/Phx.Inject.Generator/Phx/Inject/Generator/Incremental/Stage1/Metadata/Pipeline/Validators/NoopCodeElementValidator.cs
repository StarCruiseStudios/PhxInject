// -----------------------------------------------------------------------------
// <copyright file="NoopCodeElementValidator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

/// <summary>
///     A no-operation validator that accepts all code elements unconditionally.
/// </summary>
/// <remarks>
///     Null Object pattern for validation. Returns true for all elements, eliminating null checks in
///     validation pipelines. Singleton instance (stateless/immutable). Use cases: optional validation
///     steps, testing/debugging, default parameters, aggregation identity. Security note: always
///     returning true means no validation - use only when genuinely accepting all elements.
/// </remarks>
///     - Noop validator: "Accept everything, filter nothing"
///     </para>
/// </remarks>
internal sealed class NoopCodeElementValidator : ICodeElementValidator {
    /// <summary>
    ///     Singleton instance - use this instead of constructing new instances.
    /// </summary>
    /// <remarks>
    ///     Stateless validator with no configuration - one instance serves all use cases.
    /// </remarks>
    public static readonly NoopCodeElementValidator Instance = new();
    
    /// <inheritdoc />
    /// <remarks>
    ///     Always returns true, accepting all symbols including null. The NotNullWhen(true) attribute
    ///     contract is technically violated (we return true even when symbol is null), but this is
    ///     acceptable for a testing/debugging validator that bypasses normal validation logic.
    /// </remarks>
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) => true;
    
    /// <inheritdoc />
    /// <remarks>
    ///     Always returns true, accepting all syntax nodes.
    /// </remarks>
    public bool IsValidSyntax(SyntaxNode syntaxNode) => true;
    
    /// <summary>
    ///     Private constructor enforces singleton pattern.
    /// </summary>
    /// <remarks>
    ///     Use NoopCodeElementValidator.Instance instead of constructing directly.
    /// </remarks>
    private NoopCodeElementValidator() { }
}