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
///     <para>Design Purpose - Null Object Pattern:</para>
///     <para>
///     This validator implements the Null Object pattern for validation scenarios. Rather than
///     using null to represent "no validation needed", we use this singleton instance that always
///     returns true. This eliminates null checks throughout validation pipeline code.
///     </para>
///     
///     <para>WHY This Exists:</para>
///     <list type="bullet">
///         <item>
///             <term>Optional validation:</term>
///             <description>
///             Some pipelines have optional validation steps. Rather than nullable validator fields
///             and conditional logic, use NoopCodeElementValidator.Instance to represent "skip validation".
///             </description>
///         </item>
///         <item>
///             <term>Testing/debugging:</term>
///             <description>
///             During development, temporarily replace complex validators with noop validator to
///             isolate which validation is causing failures. Makes debugging easier.
///             </description>
///         </item>
///         <item>
///             <term>Default parameters:</term>
///             <description>
///             Factory methods or constructors that take validator parameters can default to
///             NoopCodeElementValidator.Instance instead of null, simplifying calling code.
///             </description>
///         </item>
///         <item>
///             <term>Aggregation identity:</term>
///             <description>
///             When building aggregate validators dynamically, noop validator acts as the identity
///             element (doesn't change the aggregate's behavior).
///             </description>
///         </item>
///     </list>
///     
///     <para>Singleton Pattern:</para>
///     <para>
///     The validator is a singleton because it's stateless and immutable. Creating multiple
///     instances would waste memory with no benefit. Private constructor prevents external
///     instantiation; callers must use NoopCodeElementValidator.Instance.
///     </para>
///     
///     <para>Security Note:</para>
///     <para>
///     Always returning true means NO validation occurs. Use this only when you genuinely want
///     to accept all code elements. Using noop validator in production validation pipelines
///     could allow malformed code through, producing incorrect generated output or compiler errors.
///     It's primarily a development/testing tool.
///     </para>
///     
///     <para>Contrast with Empty AggregateElementValidator:</para>
///     <para>
///     An AggregateElementValidator with empty validator list returns true for symbols (vacuous truth)
///     but false for syntax (no filter passed). NoopCodeElementValidator returns true for both phases.
///     They serve different purposes:
///     - Empty aggregate: "No constraints, but still filter syntax nodes"
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