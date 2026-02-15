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
/// <param name="requiredAccessibility">Required accessibility level. Defaults to Any.</param>
/// <param name="isStatic">If non-null, specifies whether field must be static.</param>
/// <param name="isReadonly">If non-null, specifies whether field must be readonly.</param>
/// <param name="requiredAttributes">Attributes that must all be present.</param>
/// <param name="prohibitedAttributes">Attributes that must not be present.</param>
/// <remarks>
///     Validates field injection patterns and injector state management. Readonly enforcement ensures
///     immutability for thread-safety. Static constraints on specifications prevent hidden state.
///     Accessibility validation ensures generated code can access fields without exposing internals.
/// </remarks>
///     why syntax validation is minimal - the syntax/symbol mapping is not 1:1.
///     </para>
///     
///     <para>Design Tradeoff - Minimal Syntax Filtering:</para>
///     <para>
///     Similar to properties, field validators perform minimal syntax filtering. Fields are relatively
///     rare as DI injection points, and the multi-declarator syntax makes parsing complex. We accept
///     all field declarations at syntax phase and rely on symbol validation for authoritative checks.
///     </para>
/// </remarks>
internal sealed class FieldElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? isStatic = null,
    bool? isReadonly = null,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null,
    IReadOnlyList<IAttributeChecker>? prohibitedAttributes = null
) : ICodeElementValidator {
    /// <summary>
    ///     Validates public or internal fields (typical specification reference field).
    /// </summary>
    public static readonly FieldElementValidator PublicField = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal);
    
    /// <summary>
    ///     Validates public or internal static fields (typical specification static reference field).
    /// </summary>
    public static readonly FieldElementValidator PublicStaticField = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: true);

    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;
    private readonly IReadOnlyList<IAttributeChecker> prohibitedAttributes = prohibitedAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    /// <inheritdoc />
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
    ///     <para>Intentionally Minimal Syntax Validation:</para>
    ///     <para>
    ///     This validator only checks that the node is a FieldDeclarationSyntax, performing no
    ///     modifier validation at syntax level. This is a deliberate design choice.
    ///     </para>
    ///     
    ///     <para>WHY Defer Everything to Symbol Phase:</para>
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
    ///     <para>Multi-Declarator Example:</para>
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
