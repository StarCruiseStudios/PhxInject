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
///     Collection of attribute checkers that must all pass. Null or empty means no attribute requirements.
/// </param>
/// <remarks>
///     <para><b>Design Purpose - Injector Interface Contracts:</b></para>
///     <para>
///     Interface validators enforce structural contracts for user-defined injector interfaces.
///     Injector interfaces declare what dependencies an application needs to obtain, with abstract
///     method signatures serving as the provider contract that the generator will implement.
///     </para>
///     
///     <para><b>WHY Validate Interfaces for DI:</b></para>
///     <list type="bullet">
///         <item>
///             <term>Separation of concerns:</term>
///             <description>
///             Interface-based injectors separate dependency declaration (interface) from implementation
///             (generated code). This allows compile-time dependency graph validation without exposing
///             implementation details.
///             </description>
///         </item>
///         <item>
///             <term>Multiple implementations:</term>
///             <description>
///             Interfaces enable generating different implementations for test vs production, or
///             different scoping strategies (singleton, scoped, transient) from the same interface.
///             </description>
///         </item>
///         <item>
///             <term>Partial class alternative:</term>
///             <description>
///             Not all languages/contexts support partial classes. Interfaces provide a universal
///             contract mechanism that works across assembly boundaries and language interop scenarios.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Validation Rules - WHY These Constraints:</b></para>
///     <list type="bullet">
///         <item>
///             <term>TypeKind == Interface:</term>
///             <description>
///             Only interfaces can have purely abstract contracts without implementation details.
///             Classes with abstract methods still have constructors, fields, and implementation
///             assumptions that complicate generation. Pure interfaces keep the contract clean.
///             </description>
///         </item>
///         <item>
///             <term>Accessibility checks:</term>
///             <description>
///             Generated implementation must be accessible where the interface is used. Private nested
///             interfaces cannot be implemented outside their declaring class, limiting their utility
///             for DI. We catch this early to prevent confusing compiler errors about inaccessible types.
///             </description>
///         </item>
///         <item>
///             <term>Attribute requirements:</term>
///             <description>
///             Attributes like @Injector signal intent and configuration to the generator. Requiring
///             specific attributes prevents accidental generation on unrelated interfaces and provides
///             metadata for generation strategy (scope, module composition, etc.).
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Syntax vs Symbol Validation Gap:</b></para>
///     <para>
///     Syntax validation can only check the 'interface' keyword and modifiers. It cannot verify
///     attribute types (is it actually @Injector or just something named similarly?). Symbol validation
///     performs the authoritative attribute check. Some nodes pass syntax but fail symbol validation -
///     this is expected and acceptable for performance reasons.
///     </para>
///     
///     <para><b>What Malformed Code Gets Caught:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Private nested interfaces that would produce inaccessible generated implementations
///             </description>
///         </item>
///         <item>
///             <description>
///             Classes, structs, or delegates mistakenly marked with @Injector attribute
///             </description>
///         </item>
///         <item>
///             <description>
///             Interfaces missing required configuration attributes for DI generation
///             </description>
///         </item>
///     </list>
/// </remarks>
internal class InterfaceElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null
) : ICodeElementValidator {
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
    ///     <para><b>Syntax-Level Tradeoffs:</b></para>
    ///     <para>
    ///     Interface declarations are simpler than classes - they cannot be static or abstract
    ///     (all members are implicitly abstract). This makes syntax validation more straightforward
    ///     but also less informative. We only check the declaration type and accessibility modifiers.
    ///     </para>
    ///     
    ///     <para><b>Conservative Accessibility Filtering:</b></para>
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