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
///     <para>Dual-Phase Validation Strategy:</para>
///     <para>
///     Validators operate in two phases corresponding to Roslyn's incremental generator pipeline:
///     </para>
///     <list type="number">
///         <item>
///             <term>Syntax-Level (Predicate Phase):</term>
///             <description>
///             IsValidSyntax executes during predicate filtering, before semantic analysis.
///             Uses only syntax tokens and modifiers to quickly filter out obviously invalid candidates.
///             Fast but limited - cannot check attribute types or resolve type information.
///             </description>
///         </item>
///         <item>
///             <term>Symbol-Level (Transform Phase):</term>
///             <description>
///             IsValidSymbol executes during transformation with full semantic model access.
///             Can check attributes, resolved types, inheritance, and semantic constraints.
///             Authoritative but slower - only runs on nodes that passed syntax validation.
///             </description>
///         </item>
///     </list>
///     
///     <para>Why Two Phases?</para>
///     <para>
///     Syntax validation eliminates 95%+ of nodes cheaply (e.g., reject all private classes when
///     we need public). Symbol validation then performs expensive semantic checks only on the small
///     remaining set. This two-pass approach is critical for acceptable IDE performance.
///     </para>
///     
///     <para>Contract Guarantees:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             If IsValidSyntax returns false, IsValidSymbol will also return false (syntax is superset filter)
///             </description>
///         </item>
///         <item>
///             <description>
///             Both methods are idempotent and side-effect free
///             </description>
///         </item>
///         <item>
///             <description>
///             Both methods must be thread-safe (Roslyn calls from parallel worker threads)
///             </description>
///         </item>
///     </list>
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
    ///     <para>When This Executes:</para>
    ///     <para>
    ///     Called during the transform phase of incremental generation, after the predicate phase
    ///     has filtered candidates via IsValidSyntax. At this point, full semantic model is available.
    ///     </para>
    ///     
    ///     <para>What to Validate:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Attribute presence and types (cannot do in syntax phase)</description>
    ///         </item>
    ///         <item>
    ///             <description>Resolved type information, generic constraints, base types</description>
    ///         </item>
    ///         <item>
    ///             <description>Semantic accessibility (considers [InternalsVisibleTo], etc.)</description>
    ///         </item>
    ///         <item>
    ///             <description>Cross-reference validation (interface implementation, etc.)</description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>NotNullWhen Attribute:</para>
    ///     <para>
    ///     The NotNullWhen(true) attribute informs the compiler that if this method returns true,
    ///     the symbol parameter is guaranteed non-null. Callers can safely dereference symbol
    ///     without additional null checks in the success path.
    ///     </para>
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
    ///     <para>When This Executes:</para>
    ///     <para>
    ///     Called during Roslyn's predicate phase, before any semantic analysis or symbol binding.
    ///     Executes on potentially millions of syntax nodes across the compilation, so performance
    ///     is critical. Must complete in microseconds per invocation.
    ///     </para>
    ///     
    ///     <para>What to Validate:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Syntax modifiers (public, static, abstract, partial, etc.)</description>
    ///         </item>
    ///         <item>
    ///             <description>Node type (ClassDeclarationSyntax vs InterfaceDeclarationSyntax, etc.)</description>
    ///         </item>
    ///         <item>
    ///             <description>Basic structural requirements (has body, has identifier, etc.)</description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>What NOT to Validate:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Attribute types (syntax doesn't resolve attribute symbols)</description>
    ///         </item>
    ///         <item>
    ///             <description>Type information (no semantic model available)</description>
    ///         </item>
    ///         <item>
    ///             <description>Cross-file references or inheritance (not bound yet)</description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>Conservative Bias:</para>
    ///     <para>
    ///     When uncertain due to incomplete information, prefer returning true (let symbol validation
    ///     decide). The cost of a false positive is one extra symbol lookup. The cost of a false
    ///     negative is silently ignoring user code.
    ///     </para>
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
    ///     <para>Short-Circuit Evaluation:</para>
    ///     <para>
    ///     The aggregate validator short-circuits on the first failing validator, avoiding
    ///     unnecessary validation work. Order validators from fastest-failing to slowest.
    ///     </para>
    ///     
    ///     <para>Use Case:</para>
    ///     <para>
    ///     Useful when multiple orthogonal constraints apply to the same code element.
    ///     For example, a class might need to be: public, partial, have specific attributes,
    ///     and implement a specific interface. Each concern gets its own validator, composed here.
    ///     </para>
    /// </remarks>
    public static ICodeElementValidator Of(params ICodeElementValidator[] validators) {
        return new AggregateElementValidator(validators);
    } 
}