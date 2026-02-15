// -----------------------------------------------------------------------------
// <copyright file="ICodeElementValidator.cs" company="Star Cruise Studios LLC">
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
///     Validates that code elements (symbols and syntax) conform to required structural constraints.
/// </summary>
/// <remarks>
///     Dual-phase validation: (1) <c>IsValidSyntax</c> - fast syntax-only predicate filtering before
///     semantic analysis; (2) <c>IsValidSymbol</c> - authoritative semantic validation with type
///     resolution. Syntax rejects 95%+ nodes cheaply; symbol validates remaining set. Both methods
///     idempotent, side-effect free, thread-safe. Syntax false implies symbol false (superset filter).
/// </remarks>
internal interface ICodeElementValidator {
    /// <summary>
    ///     Validates whether a symbol meets semantic requirements after full type resolution.
    /// </summary>
    /// <param name="symbol">
    ///     The Roslyn symbol to validate. May be null if semantic analysis failed.
    /// </param>
    /// <returns>
    ///     True if the symbol meets all validation criteria and should proceed to metadata extraction.
    ///     False if the symbol violates requirements or is null.
    /// </returns>
    /// <remarks>
    ///     Transform phase with full semantic model. Validates: attribute presence/types, resolved type
    ///     info, semantic accessibility, cross-references. <c>NotNullWhen(true)</c> guarantees non-null
    ///     symbol on success.
    /// </remarks>
    bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol);
    
    /// <summary>
    ///     Performs fast syntax-only validation without semantic analysis.
    /// </summary>
    /// <param name="syntaxNode">
    ///     The syntax node to validate. Never null during predicate phase.
    /// </param>
    /// <returns>
    ///     True if the syntax node might be valid (proceed to transform phase for symbol validation).
    ///     False if the syntax definitively violates requirements (skip transform phase entirely).
    /// </returns>
    /// <remarks>
    ///     Predicate phase before semantic analysis. Performance critical (executes on millions of nodes).
    ///     Validates: syntax modifiers, node type, basic structure. Cannot validate: attribute types,
    ///     type info, cross-file references. Conservative bias: uncertain = return true (symbol decides).
    /// </remarks>
    bool IsValidSyntax(SyntaxNode syntaxNode);
}

/// <summary>
///     Factory methods for composing validators without direct constructor access.
/// </summary>
internal static class CodeElementValidator {
    /// <summary>
    ///     Combines multiple validators into a single validator requiring all checks to pass.
    /// </summary>
    /// <param name="validators">
    ///     The validators to combine. Empty array produces a validator that always returns true.
    /// </param>
    /// <returns>
    ///     An aggregate validator implementing AND logic across all provided validators.
    /// </returns>
    /// <remarks>
    ///     Short-circuits on first failing validator. Order fastest-failing first for best performance.
    /// </remarks>
    public static ICodeElementValidator Of(params ICodeElementValidator[] validators) {
        return new AggregateElementValidator(validators);
    } 
}