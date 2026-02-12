// -----------------------------------------------------------------------------
// <copyright file="FieldElementValidator.cs" company="Star Cruise Studios LLC">
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
///     Validates that a field symbol meets specified structural and semantic requirements.
/// </summary>
/// <param name="requiredAccessibility">
///     Required accessibility level (Any, PublicOrInternal, etc.). Defaults to Any.
/// </param>
/// <param name="isStatic">
///     If non-null, specifies whether field must be static (true) or instance (false).
/// </param>
/// <param name="isReadonly">
///     If non-null, specifies whether field must be readonly (true) or mutable (false).
/// </param>
/// <param name="requiredAttributes">
///     Attributes that must all be present. Null/empty means no attribute requirements.
/// </param>
/// <param name="prohibitedAttributes">
///     Attributes that must not be present. Null/empty means no prohibited attributes.
/// </param>
/// <remarks>
///     <para><b>Design Purpose - Field Injection and State Management:</b></para>
///     <para>
///     Field validators enable validation of field injection patterns and stateful injector implementations.
///     Key DI patterns supported:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Field Injection:</term>
///             <description>
///             Private or protected fields marked with @Inject, initialized after construction.
///             Controversial in modern DI (constructor injection preferred) but supported for legacy
///             code and framework compatibility.
///             </description>
///         </item>
///         <item>
///             <term>Injector State Fields:</term>
///             <description>
///             Generated injector implementations maintain internal state (cached singletons, lazy instances)
///             in private readonly fields. Validation ensures these follow immutability patterns.
///             </description>
///         </item>
///         <item>
///             <term>Specification Constants:</term>
///             <description>
///             Static readonly fields on specification classes providing compile-time constants or
///             default configurations that factories can reference.
///             </description>
///         </item>
///         <item>
///             <term>Anti-pattern Detection:</term>
///             <description>
///             Non-readonly instance fields on specification classes violate statelessness assumptions.
///             Validation catches this design mistake early.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>WHY These Constraints Exist:</b></para>
///     <list type="bullet">
///         <item>
///             <term>Readonly enforcement:</term>
///             <description>
///             Readonly fields ensure thread-safety and immutability. In DI contexts, mutable fields
///             introduce hidden state that's not visible in dependency graphs, making behavior
///             unpredictable. Enforcing readonly catches accidental mutation bugs.
///             </description>
///         </item>
///         <item>
///             <term>Static constraint:</term>
///             <description>
///             Specification classes should be stateless static containers. Instance fields imply
///             hidden state that violates this design principle. Enforcing static-only prevents
///             accidental conversion of stateless specifications into stateful objects.
///             </description>
///         </item>
///         <item>
///             <term>Accessibility validation:</term>
///             <description>
///             Field injection requires generator to access the field. Public fields on injector
///             implementations expose internal state, violating encapsulation. Private fields on
///             specification classes may be constants the generator needs to read. Accessibility
///             validation ensures generated code can perform its role without exposing internals.
///             </description>
///         </item>
///         <item>
///             <term>Attribute-based discrimination:</term>
///             <description>
///             Not all fields in a class are injection targets. @Inject attribute (required) or
///             @Ignore attribute (prohibited) let users control which fields participate in DI,
///             preventing accidental injection of unrelated fields.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>What Malformed Code Gets Caught:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Mutable instance fields on specification classes (violates statelessness)
///             </description>
///         </item>
///         <item>
///             <description>
///             @Inject on private fields in sealed classes (generator cannot access via reflection)
///             </description>
///         </item>
///         <item>
///             <description>
///             Public mutable fields on generated injectors (breaks encapsulation)
///             </description>
///         </item>
///         <item>
///             <description>
///             Conflicting attributes (both @Inject and @Ignore simultaneously)
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Field Injection Controversy:</b></para>
///     <para>
///     Field injection is considered an anti-pattern in modern DI frameworks (constructor injection
///     strongly preferred for testability and immutability). However, this validator exists to support
///     legacy codebases, framework compatibility (some frameworks require field injection), and
///     pragmatic scenarios where constructor bloat becomes problematic. Users should prefer constructor
///     or property injection when possible.
///     </para>
///     
///     <para><b>Syntax vs Symbol Validation Gap:</b></para>
///     <para>
///     Field declarations can declare multiple variables in one statement (int x, y, z;). Syntax
///     validation only sees the FieldDeclarationSyntax, not individual field symbols. All modifier
///     validation defers to symbol phase where each field has a distinct IFieldSymbol. This is
///     why syntax validation is minimal - the syntax/symbol mapping is not 1:1.
///     </para>
///     
///     <para><b>Design Tradeoff - Minimal Syntax Filtering:</b></para>
///     <para>
///     Similar to properties, field validators perform minimal syntax filtering. Fields are relatively
///     rare as DI injection points, and the multi-declarator syntax makes parsing complex. We accept
///     all field declarations at syntax phase and rely on symbol validation for authoritative checks.
///     </para>
/// </remarks>
internal class FieldElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? isStatic = null,
    bool? isReadonly = null,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null,
    IReadOnlyList<IAttributeChecker>? prohibitedAttributes = null
) : ICodeElementValidator {
    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;
    private readonly IReadOnlyList<IAttributeChecker> prohibitedAttributes = prohibitedAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    /// <inheritdoc />
    /// <remarks>
    ///     Executes checks in order from fastest to slowest: type check, attributes, accessibility,
    ///     then static and readonly modifiers. This ordering maximizes short-circuit efficiency.
    /// </remarks>
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) {
        if (symbol is not IFieldSymbol fieldSymbol) {
            return false;
        }
        
        if (requiredAttributes.Any(requiredAttribute => !requiredAttribute.HasAttribute(symbol))) {
            return false;
        }
        
        if (prohibitedAttributes.Any(prohibitedAttribute => prohibitedAttribute.HasAttribute(symbol))) {
            return false;
        }

        if (!requiredAccessibility.AccessibilityMatches(fieldSymbol.DeclaredAccessibility)) {
            return false;
        }

        if (isStatic != null && isStatic != fieldSymbol.IsStatic) {
            return false;
        }

        if (isReadonly != null && isReadonly != fieldSymbol.IsReadOnly) {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para><b>Intentionally Minimal Syntax Validation:</b></para>
    ///     <para>
    ///     This validator only checks that the node is a FieldDeclarationSyntax, performing no
    ///     modifier validation at syntax level. This is a deliberate design choice.
    ///     </para>
    ///     
    ///     <para><b>WHY Defer Everything to Symbol Phase:</b></para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///             Field declarations can declare multiple fields in one statement: 'int x, y, z;'
    ///             Each field gets a separate IFieldSymbol, but there's only one FieldDeclarationSyntax.
    ///             Syntax-level validation would apply to all fields in the declaration, which may not
    ///             match per-field validation requirements.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Modifiers on FieldDeclarationSyntax apply to the declaration, not necessarily to
    ///             each field uniformly after semantic analysis. Symbol-level validation is authoritative
    ///             for modifier checks.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Fields are rare as DI injection points (constructor/property injection preferred),
    ///             so performance benefit of syntax filtering is minimal. Simplicity wins over optimization.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para><b>Multi-Declarator Example:</b></para>
    ///     <para>
    ///     Given: 'readonly int x = 1, y = 2;' - This is one FieldDeclarationSyntax but produces two
    ///     IFieldSymbol instances. Syntax validation runs once, symbol validation runs twice.
    ///     Any per-field constraint (like different attributes on x vs y) cannot be validated at syntax level.
    ///     </para>
    /// </remarks>
    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        if (syntaxNode is not FieldDeclarationSyntax) {
            return false;
        }

        return true;
    }
}
