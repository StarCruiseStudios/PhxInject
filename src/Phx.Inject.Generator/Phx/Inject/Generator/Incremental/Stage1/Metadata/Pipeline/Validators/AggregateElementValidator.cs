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
///     <para><b>Design Purpose - Compositional Validation:</b></para>
///     <para>
///     Aggregate validators enable separation of concerns by combining orthogonal validation rules.
///     Rather than a monolithic validator with complex branching logic, each constraint gets its
///     own focused validator, then they're composed via aggregation.
///     </para>
///     
///     <para><b>WHY Two Different Logical Operators (AND vs OR):</b></para>
///     <para>
///     The asymmetry between symbol validation (AND) and syntax validation (OR) reflects their
///     different roles in the dual-phase validation pipeline:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Symbol Validation (AND logic):</term>
///             <description>
///             Symbol validation is authoritative - all constraints must be satisfied for the element
///             to be valid. If you need a class to be: public, partial, have @Injector attribute,
///             and implement IDisposable, then ALL conditions must be true. This is conjunction logic.
///             </description>
///         </item>
///         <item>
///             <term>Syntax Validation (OR logic):</term>
///             <description>
///             Syntax validation is a conservative pre-filter. If ANY validator thinks the syntax
///             MIGHT be valid, we proceed to symbol validation to get the authoritative answer.
///             This prevents false negatives where we prematurely reject valid code due to incomplete
///             syntax-level information.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Example Scenario:</b></para>
///     <para>
///     Validating classes that are: (public OR internal) AND (partial OR abstract) AND (has @Injector).
///     - Symbol phase: All three conditions evaluated with AND (all must be true)
///     - Syntax phase: (has public/internal keyword) OR (has partial keyword) OR (has abstract keyword) OR (has any attribute)
///     </para>
///     <para>
///     The OR in syntax is conservative - if we see ANY promising signal, proceed to symbol validation.
///     </para>
///     
///     <para><b>Short-Circuit Evaluation:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Symbol validation short-circuits on first failing validator (AND logic stops at first false)
///             </description>
///         </item>
///         <item>
///             <description>
///             Syntax validation short-circuits on first succeeding validator (OR logic stops at first true)
///             </description>
///         </item>
///     </list>
///     <para>
///     This means validator ordering affects performance: for symbols, put fastest-failing first;
///     for syntax, put most-likely-to-match first.
///     </para>
///     
///     <para><b>Empty Collection Edge Case:</b></para>
///     <para>
///     An aggregate with zero validators is unusual but handled gracefully:
///     - IsValidSymbol returns true (vacuous truth: all zero constraints are satisfied)
///     - IsValidSyntax returns false (no validator said to proceed, so don't proceed)
///     </para>
///     <para>
///     This is rarely useful in practice but maintains logical consistency with LINQ's .All() and .Any() semantics.
///     </para>
///     
///     <para><b>Use Cases:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Combining accessibility + attribute + modifier constraints on classes
///             </description>
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
///     <para><b>Alternative Design Rejected:</b></para>
///     <para>
///     An earlier design used AND for both phases. This was rejected because syntax-phase false negatives
///     would silently skip valid code. The current OR-in-syntax design errs on the side of caution,
///     accepting more false positives (filtered by symbol validation) rather than false negatives.
///     </para>
/// </remarks>
internal class AggregateElementValidator(
    IReadOnlyList<ICodeElementValidator> validators
) : ICodeElementValidator {
    /// <inheritdoc />
    /// <remarks>
    ///     <para><b>AND Logic - All Must Pass:</b></para>
    ///     <para>
    ///     Returns true only if ALL validators return true. Uses LINQ's .All() which short-circuits
    ///     on first false result, avoiding unnecessary validation work.
    ///     </para>
    ///     
    ///     <para><b>Empty Collection:</b></para>
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
    ///     <para><b>OR Logic - Any Can Pass:</b></para>
    ///     <para>
    ///     Returns true if ANY validator returns true. Uses LINQ's .Any() which short-circuits
    ///     on first true result.
    ///     </para>
    ///     
    ///     <para><b>WHY OR Instead of AND:</b></para>
    ///     <para>
    ///     Syntax validation is a conservative pre-filter with incomplete information. If any
    ///     validator sees a promising signal (e.g., has 'partial' keyword, has an attribute,
    ///     has public modifier), we should proceed to symbol validation. AND logic would risk
    ///     false negatives where we reject valid code because one validator couldn't confirm
    ///     from syntax alone.
    ///     </para>
    ///     
    ///     <para><b>Empty Collection:</b></para>
    ///     <para>
    ///     If validators collection is empty, .Any() returns false. This represents "no filter
    ///     passed, don't proceed" which is safe default behavior.
    ///     </para>
    /// </remarks>
    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        return validators.Any(validator => validator.IsValidSyntax(syntaxNode));
    }
}