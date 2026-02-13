// -----------------------------------------------------------------------------
// <copyright file="ClassElementValidator.cs" company="Star Cruise Studios LLC">
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
///     Validates that a type symbol represents a class meeting specified structural requirements.
/// </summary>
/// <param name="requiredAccessibility">
///     The accessibility level required for the class. Defaults to Any (no accessibility constraint).
/// </param>
/// <param name="isStatic">
///     If non-null, specifies whether the class must be static (true) or non-static (false).
///     Null means no constraint on static modifier.
/// </param>
/// <param name="isAbstract">
///     If non-null, specifies whether the class must be abstract (true) or concrete (false).
///     Null means no constraint on abstract modifier.
/// </param>
/// <param name="requiredAttributes">
///     Collection of attribute checkers that must all pass. Null or empty means no attribute requirements.
/// </param>
/// <remarks>
///     <para>Common Usage Patterns:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Public partial classes for injectors: PublicOrInternal accessibility, no other constraints
///             </description>
///         </item>
///         <item>
///             <description>
///             Static specification classes: isStatic=true, required @Specification attribute
///             </description>
///         </item>
///         <item>
///             <description>
///             Abstract base specs: isAbstract=true, required @Specification attribute
///             </description>
///         </item>
///     </list>
///     
///     <para>Validation Rules - WHY These Constraints:</para>
///     <list type="bullet">
///         <item>
///             <term>TypeKind == Class:</term>
///             <description>
///             Interfaces, structs, delegates cannot have the semantics we need (factories need
///             implementations, injectors need fields for instance state). Enforcing class prevents
///             user confusion from applying attributes to wrong constructs.
///             </description>
///         </item>
///         <item>
///             <term>Accessibility checks:</term>
///             <description>
///             Generated code must be able to reference the class. Private classes in nested contexts
///             would produce uncompilable output. We catch this early with clear diagnostics.
///             </description>
///         </item>
///         <item>
///             <term>Static requirement:</term>
///             <description>
///             Specification classes should be static to signal they're pure factory containers
///             with no instance state. Enforcing this catches accidental instance fields that
///             would violate statelessness assumptions.
///             </description>
///         </item>
///         <item>
///             <term>Abstract/concrete distinction:</term>
///             <description>
///             Abstract specs can define partial graphs that child specs complete. Concrete specs
///             must provide complete dependency graphs. This distinction enables hierarchical
///             specifications.
///             </description>
///         </item>
///     </list>
///     
///     <para>Syntax vs Symbol Validation Gap:</para>
///     <para>
///     Syntax validation cannot check attribute types (e.g., is @Specification present?) because
///     attributes aren't resolved yet. The syntax phase only filters by modifiers like 'static' and
///     'abstract'. Symbol validation then performs the authoritative attribute check. This means
///     some nodes pass syntax but fail symbol - this is intentional and acceptable.
///     </para>
///     
///     <para>Error Reporting:</para>
///     <para>
///     This validator returns boolean only - it doesn't generate diagnostics. Diagnostic generation
///     happens in the transformer that uses this validator. This separation keeps validation logic
///     pure and reusable across different diagnostic strategies.
///     </para>
/// </remarks>
internal class ClassElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? isStatic = null,
    bool? isAbstract = null,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null
) : ICodeElementValidator {
    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    /// <inheritdoc />
    /// <remarks>
    ///     Executes checks in order from fastest to slowest to maximize short-circuit efficiency:
    ///     type check, attribute checks, accessibility, then modifier flags.
    /// </remarks>
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) {
        if (symbol is not ITypeSymbol typeSymbol) {
            return false;
        }
        
        if (requiredAttributes.Any(requiredAttribute => !requiredAttribute.HasAttribute(symbol))) {
            return false;
        }

        if (typeSymbol.TypeKind != TypeKind.Class) {
            return false;
        }

        if (!requiredAccessibility.AccessibilityMatches(typeSymbol.DeclaredAccessibility)) {
            return false;
        }

        if (isStatic != null && isStatic != typeSymbol.IsStatic) {
            return false;
        }

        if (isAbstract != null && isAbstract != typeSymbol.IsAbstract) {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>Limitation - Attributes Cannot Be Checked:</para>
    ///     <para>
    ///     The syntax-level validation cannot verify attribute types because attributes are just
    ///     syntax nodes here, not resolved symbols. We accept this imprecision - if a class has
    ///     the right modifiers but wrong attributes, it will pass syntax but fail symbol validation.
    ///     The wasted symbol lookup is acceptable given the performance benefit of early filtering.
    ///     </para>
    ///     
    ///     <para>Accessibility Translation:</para>
    ///     <para>
    ///     Syntax modifiers don't map 1:1 to semantic accessibility. For example, a class with no
    ///     modifiers is internally accessible by default. We use heuristics here: if PublicOrInternal
    ///     is required, we reject anything explicitly marked private/protected but accept anything else
    ///     (letting symbol validation make the final determination).
    ///     </para>
    /// </remarks>
    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration) {
            return false;
        }

        // Note: requiredAttributes check cannot be performed on syntax nodes
        // as attributes are not fully resolved at this stage

        switch (requiredAccessibility) {
            case CodeElementAccessibility.PublicOrInternal:
                // All modifiers must not be private or protected
                if (!classDeclaration.Modifiers
                    .All(modifier => modifier.ValueText switch {
                        "private" or "protected" => false,
                        "internal" or "public" => true,
                        _ => true
                    })) {
                    return false;
                }
                break;
            case CodeElementAccessibility.Any:
                break;
            default:
                // Unknown accessibility, treat as invalid
                return false;
        }

        if (isStatic != null) {
            var hasStaticModifier = classDeclaration.Modifiers.Any(modifier => modifier.ValueText == "static");
            if (isStatic != hasStaticModifier) {
                return false;
            }
        }

        if (isAbstract != null) {
            var hasAbstractModifier = classDeclaration.Modifiers.Any(modifier => modifier.ValueText == "abstract");
            if (isAbstract != hasAbstractModifier) {
                return false;
            }
        }

        return true;
    }
}