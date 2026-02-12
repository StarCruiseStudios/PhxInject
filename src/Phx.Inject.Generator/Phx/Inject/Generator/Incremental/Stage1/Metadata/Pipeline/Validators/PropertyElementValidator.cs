// -----------------------------------------------------------------------------
// <copyright file="PropertyElementValidator.cs" company="Star Cruise Studios LLC">
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
///     Validates that a property symbol meets specified structural and semantic requirements.
/// </summary>
/// <param name="requiredAccessibility">
///     Required accessibility level (Any, PublicOrInternal, etc.). Defaults to Any.
/// </param>
/// <param name="isStatic">
///     If non-null, specifies whether property must be static (true) or instance (false).
/// </param>
/// <param name="hasGetter">
///     If non-null, specifies whether property must have a getter (true) or not (false).
/// </param>
/// <param name="hasSetter">
///     If non-null, specifies whether property must have a setter (true) or not (false).
/// </param>
/// <param name="isRequired">
///     If non-null, specifies whether property must have the 'required' modifier (C# 11+).
/// </param>
/// <param name="requiredAttributes">
///     Attributes that must all be present. Null/empty means no attribute requirements.
/// </param>
/// <param name="prohibitedAttributes">
///     Attributes that must not be present. Null/empty means no prohibited attributes.
/// </param>
/// <remarks>
///     <para><b>Design Purpose - Property Injection Patterns:</b></para>
///     <para>
///     Property validators enable validation of setter injection and property-based configuration patterns.
///     Key DI patterns supported:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Property Injection:</term>
///             <description>
///             Public settable properties marked with @Inject. Allows optional dependencies and post-construction
///             initialization. Requires setter accessibility validation.
///             </description>
///         </item>
///         <item>
///             <term>Configuration Properties:</term>
///             <description>
///             Read-only properties on specification objects that expose configuration values.
///             Requires getter presence, may prohibit setters for immutability.
///             </description>
///         </item>
///         <item>
///             <term>Required Properties (C# 11):</term>
///             <description>
///             Properties marked 'required' ensure object initialization completeness. Validation ensures
///             generated code satisfies required property constraints.
///             </description>
///         </item>
///         <item>
///             <term>Static Context Properties:</term>
///             <description>
///             Static properties on specification classes providing global constants or ambient context.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>WHY These Constraints Exist:</b></para>
///     <list type="bullet">
///         <item>
///             <term>Setter requirement:</term>
///             <description>
///             Property injection requires a setter to actually inject the dependency. Getter-only
///             properties cannot be injected after construction. Enforcing this prevents confusing
///             situations where @Inject on a read-only property silently does nothing.
///             </description>
///         </item>
///         <item>
///             <term>Getter requirement:</term>
///             <description>
///             Configuration properties that will be read by generated code must have getters.
///             Write-only properties cannot provide dependency information to the generator.
///             </description>
///         </item>
///         <item>
///             <term>Static constraint:</term>
///             <description>
///             Instance properties imply state, static properties imply configuration. Specification
///             patterns often require static-only members to ensure statelessness and avoid accidental
///             instance dependencies.
///             </description>
///         </item>
///         <item>
///             <term>Required modifier check:</term>
///             <description>
///             C# 11 'required' properties must be initialized during object construction. Generated
///             code must honor this contract or produce compiler errors. Validation ensures the generator
///             can satisfy required property constraints.
///             </description>
///         </item>
///         <item>
///             <term>Accessibility validation:</term>
///             <description>
///             Generated code must be able to access the property's setter/getter. Private properties
///             cannot be injected or read from external generated code. Catching this early provides
///             clear diagnostics rather than obscure compiler errors.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>What Malformed Code Gets Caught:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             @Inject attributes on read-only properties (cannot inject without setter)
///             </description>
///         </item>
///         <item>
///             <description>
///             Configuration properties without getters (cannot read configuration)
///             </description>
///         </item>
///         <item>
///             <description>
///             Instance properties on specification classes (violates statelessness)
///             </description>
///         </item>
///         <item>
///             <description>
///             Private properties marked for injection (inaccessible to generated code)
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Syntax vs Symbol Validation Gap:</b></para>
///     <para>
///     Syntax validation cannot determine getter/setter presence or accessibility from PropertyDeclarationSyntax
///     without analyzing the accessor list in detail. Rather than duplicate complex parsing logic, we defer
///     all property-specific validation to the symbol phase. Syntax phase only confirms it's a property
///     declaration. This trades some syntax-level filtering for simpler, more maintainable code.
///     </para>
///     
///     <para><b>Design Tradeoff - Minimal Syntax Filtering:</b></para>
///     <para>
///     Unlike class or method validators, this validator performs minimal syntax filtering (only type check).
///     This is intentional: property syntax is complex (auto-properties, expression-bodied, init-only setters)
///     and attempting to validate modifiers at syntax level would be brittle and error-prone. We accept higher
///     false-positive rate at syntax phase in exchange for correctness and maintainability.
///     </para>
/// </remarks>
internal class PropertyElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? isStatic = null,
    bool? hasGetter = null,
    bool? hasSetter = null,
    bool? isRequired = null,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null,
    IReadOnlyList<IAttributeChecker>? prohibitedAttributes = null
) : ICodeElementValidator {
    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;
    private readonly IReadOnlyList<IAttributeChecker> prohibitedAttributes = prohibitedAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    /// <inheritdoc />
    /// <remarks>
    ///     Executes checks in order from fastest to slowest: type check, attributes, accessibility,
    ///     then modifier and accessor checks. This ordering maximizes short-circuit efficiency.
    /// </remarks>
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) {
        if (symbol is not IPropertySymbol propertySymbol) {
            return false;
        }

        if (requiredAttributes.Any(requiredAttribute => !requiredAttribute.HasAttribute(symbol))) {
            return false;
        }
        
        if (prohibitedAttributes.Any(prohibitedAttribute => prohibitedAttribute.HasAttribute(symbol))) {
            return false;
        }

        if (!requiredAccessibility.AccessibilityMatches(propertySymbol.DeclaredAccessibility)) {
            return false;
        }

        if (isStatic != null && isStatic != propertySymbol.IsStatic) {
            return false;
        }

        if (hasGetter != null && hasGetter != (propertySymbol.GetMethod != null)) {
            return false;
        }

        if (hasSetter != null && hasSetter != (propertySymbol.SetMethod != null)) {
            return false;
        }

        if (isRequired != null && isRequired != propertySymbol.IsRequired) {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para><b>Intentionally Minimal Syntax Validation:</b></para>
    ///     <para>
    ///     This validator only checks that the node is a PropertyDeclarationSyntax, performing no
    ///     modifier or accessor validation at syntax level. This is a deliberate design choice.
    ///     </para>
    ///     
    ///     <para><b>WHY Defer Everything to Symbol Phase:</b></para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///             Property syntax varies widely: auto-properties, expression-bodied, init-only setters,
    ///             explicit accessors with different modifiers, getter-only with expression body, etc.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Determining getter/setter presence requires parsing accessor lists, which can have
    ///             complex scenarios (private setter, init setter, etc.). Roslyn's symbol APIs handle
    ///             this complexity correctly; duplicating this logic would be brittle.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Properties are relatively rare compared to methods in typical code, so the performance
    ///             benefit of syntax-level filtering is minimal. The complexity cost exceeds the benefit.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para><b>Performance Tradeoff:</b></para>
    ///     <para>
    ///     This means every PropertyDeclarationSyntax proceeds to symbol validation, even if it will
    ///     ultimately fail. In practice, this is acceptable because: (1) properties are uncommon in
    ///     DI specification code, (2) symbol lookup cost is amortized across multiple validations,
    ///     (3) simpler code is easier to maintain and less likely to have bugs.
    ///     </para>
    /// </remarks>
    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        if (syntaxNode is not PropertyDeclarationSyntax) {
            return false;
        }

        return true;
    }
}
