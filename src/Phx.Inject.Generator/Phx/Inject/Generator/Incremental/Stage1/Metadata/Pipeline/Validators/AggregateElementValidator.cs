// -----------------------------------------------------------------------------
// <copyright file="AggregateElementValidator.cs" company="Star Cruise Studios LLC">
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
///     Combines multiple validators into a single validator using AND logic for symbols and OR logic for syntax.
/// </summary>
/// <param name="validators">
///     Collection of validators to combine. Empty collection produces a validator that always returns true
///     for symbols and false for syntax.
/// </param>
/// <remarks>
///     Symbol validation uses AND (all constraints must pass). Syntax validation uses OR (any validator
///     passing means proceed to symbol phase). Short-circuits: symbol stops at first false, syntax stops
///     at first true. Enables compositional validation separating orthogonal concerns.
/// </remarks>
///         </item>
///         <item>
///             <description>
///             Validating methods with both structural (static, non-void) and semantic (has @Factory) constraints
///             </description>
///         </item>
///         <item>
///             <description>
///             Implementing "this AND that" validation without creating specialized validator classes
///             </description>
///         </item>
///     </list>
///     
///     <para>Alternative Design Rejected:</para>
///     <para>
///     An earlier design used AND for both phases. This was rejected because syntax-phase false negatives
///     would silently skip valid code. The current OR-in-syntax design errs on the side of caution,
///     accepting more false positives (filtered by symbol validation) rather than false negatives.
///     </para>
/// </remarks>
internal sealed class AggregateElementValidator(
    IReadOnlyList<ICodeElementValidator> validators
) : ICodeElementValidator {
    /// <inheritdoc />
    /// <remarks>
    ///     <para>AND Logic - All Must Pass:</para>
    ///     <para>
    ///     Returns true only if ALL validators return true. Uses LINQ's .All() which short-circuits
    ///     on first false result, avoiding unnecessary validation work.
    ///     </para>
    ///     
    ///     <para>Empty Collection:</para>
    ///     <para>
    ///     If validators collection is empty, .All() returns true (vacuous truth).
    ///     This represents "no constraints" rather than "reject everything".
    ///     </para>
    /// </remarks>
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) {
        return validators.All(validator => validator.IsValidSymbol(symbol));
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>OR Logic - Any Can Pass:</para>
    ///     <para>
    ///     Returns true if ANY validator returns true. Uses LINQ's .Any() which short-circuits
    ///     on first true result.
    ///     </para>
    ///     
    ///     <para>WHY OR Instead of AND:</para>
    ///     <para>
    ///     Syntax validation is a conservative pre-filter with incomplete information. If any
    ///     validator sees a promising signal (e.g., has 'partial' keyword, has an attribute,
    ///     has public modifier), we should proceed to symbol validation. AND logic would risk
    ///     false negatives where we reject valid code because one validator couldn't confirm
    ///     from syntax alone.
    ///     </para>
    ///     
    ///     <para>Empty Collection:</para>
    ///     <para>
    ///     If validators collection is empty, .Any() returns false. This represents "no filter
    ///     passed, don't proceed" which is safe default behavior.
    ///     </para>
    /// </remarks>
    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        return validators.Any(validator => validator.IsValidSyntax(syntaxNode));
    }
}