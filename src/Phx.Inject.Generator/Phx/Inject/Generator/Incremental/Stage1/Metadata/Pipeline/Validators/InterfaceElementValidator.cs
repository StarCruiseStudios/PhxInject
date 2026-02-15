// -----------------------------------------------------------------------------
// <copyright file="InterfaceElementValidator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

/// <summary>
///     Validates that a type symbol represents an interface meeting specified structural requirements.
/// </summary>
/// <param name="requiredAccessibility">
///     The accessibility level required for the interface. Defaults to Any (no accessibility constraint).
/// </param>
/// <param name="requiredAttributes">
///     Attribute checkers that must all pass. Null or empty means no attribute requirements.
/// </param>
/// <remarks>
///     Enforces structural contracts for user-defined injector interfaces. Interface-based injectors
///     separate dependency declaration from implementation, enabling compile-time validation and
///     multiple implementations. Catches inaccessible interfaces and misapplied attributes.
/// </remarks>
internal sealed class InterfaceElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null
) : ICodeElementValidator {
    /// <summary>
    ///     Validates public or internal interfaces (typical injector interface).
    /// </summary>
    public static readonly InterfaceElementValidator PublicInterface = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal);

    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    /// <inheritdoc />
    /// <remarks>
    ///     Executes checks in order from fastest to slowest: type check first, then attributes,
    ///     then accessibility. This ordering maximizes short-circuit efficiency.
    /// </remarks>
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) {
        if (symbol is not ITypeSymbol typeSymbol) {
            return false;
        }
        
        if (requiredAttributes.Any(requiredAttribute => !requiredAttribute.HasAttribute(symbol))) {
            return false;
        }

        if (typeSymbol.TypeKind != TypeKind.Interface) {
            return false;
        }

        if (!requiredAccessibility.AccessibilityMatches(typeSymbol.DeclaredAccessibility)) {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>Syntax-Level Tradeoffs:</para>
    ///     <para>
    ///     Interface declarations are simpler than classes - they cannot be static or abstract
    ///     (all members are implicitly abstract). This makes syntax validation more straightforward
    ///     but also less informative. We only check the declaration type and accessibility modifiers.
    ///     </para>
    ///     
    ///     <para>Conservative Accessibility Filtering:</para>
    ///     <para>
    ///     When checking PublicOrInternal accessibility, we reject only explicit private/protected modifiers.
    ///     Interfaces without explicit accessibility modifiers default to internal, which passes our filter.
    ///     This conservative approach minimizes false negatives at the cost of some false positives that
    ///     symbol validation will catch.
    ///     </para>
    /// </remarks>
    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        if (syntaxNode is not InterfaceDeclarationSyntax interfaceDeclaration) {
            return false;
        }

        // Note: requiredAttributes check cannot be performed on syntax nodes
        // as attributes are not fully resolved at this stage

        switch (requiredAccessibility) {
            case CodeElementAccessibility.PublicOrInternal:
                // All modifiers must not be private or protected
                return interfaceDeclaration.Modifiers
                    .All(modifier => modifier.ValueText switch {
                        "private" or "protected" => false,
                        "internal" or "public" => true,
                        _ => true
                    });
            case CodeElementAccessibility.Any:
                return true;
            default:
                // Unknown accessibility, treat as invalid
                return false;
        }
    }
}