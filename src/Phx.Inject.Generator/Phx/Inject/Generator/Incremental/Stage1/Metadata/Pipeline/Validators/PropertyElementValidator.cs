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
/// <param name="requiredAccessibility">Required accessibility level. Defaults to Any.</param>
/// <param name="isStatic">If non-null, specifies whether property must be static.</param>
/// <param name="hasGetter">If non-null, specifies whether property must have a getter.</param>
/// <param name="hasSetter">If non-null, specifies whether property must have a setter.</param>
/// <param name="isRequired">If non-null, specifies whether property must have the 'required' modifier (C# 11+).</param>
/// <param name="requiredAttributes">Attributes that must all be present.</param>
/// <param name="prohibitedAttributes">Attributes that must not be present.</param>
/// <remarks>
///     Validates property injection patterns and configuration properties. Setter requirement ensures
///     injection is possible, getter requirement ensures configuration is readable. Static constraint
///     prevents hidden state. Required modifier handling ensures generated code honors C# 11 contracts.
/// </remarks>
///     all property-specific validation to the symbol phase. Syntax phase only confirms it's a property
///     declaration. This trades some syntax-level filtering for simpler, more maintainable code.
///     </para>
///     
///     <para>Design Tradeoff - Minimal Syntax Filtering:</para>
///     <para>
///     Unlike class or method validators, this validator performs minimal syntax filtering (only type check).
///     This is intentional: property syntax is complex (auto-properties, expression-bodied, init-only setters)
///     and attempting to validate modifiers at syntax level would be brittle and error-prone. We accept higher
///     false-positive rate at syntax phase in exchange for correctness and maintainability.
///     </para>
/// </remarks>
internal sealed class PropertyElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? isStatic = null,
    bool? hasGetter = null,
    bool? hasSetter = null,
    bool? isRequired = null,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null,
    IReadOnlyList<IAttributeChecker>? prohibitedAttributes = null
) : ICodeElementValidator {
    /// <summary>
    ///     Validates public/internal instance properties with getter
    ///     (typical auto-factory required property or specification factory property).
    /// </summary>
    public static readonly PropertyElementValidator PublicInstancePropertyWithGetter = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: false,
        hasGetter: true);
    
    /// <summary>
    ///     Validates public/internal instance properties with getter but no setter
    ///     (typical specification factory property - read-only).
    /// </summary>
    public static readonly PropertyElementValidator PublicReadOnlyProperty = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: false,
        hasGetter: true,
        hasSetter: false);

    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;
    private readonly IReadOnlyList<IAttributeChecker> prohibitedAttributes = prohibitedAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    /// <inheritdoc />
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
    ///     <para>Intentionally Minimal Syntax Validation:</para>
    ///     <para>
    ///     This validator only checks that the node is a PropertyDeclarationSyntax, performing no
    ///     modifier or accessor validation at syntax level. This is a deliberate design choice.
    ///     </para>
    ///     
    ///     <para>WHY Defer Everything to Symbol Phase:</para>
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
    ///     <para>Performance Tradeoff:</para>
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
