// -----------------------------------------------------------------------------
// <copyright file="PropertyElementValidator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

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

    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        if (syntaxNode is not PropertyDeclarationSyntax) {
            return false;
        }

        return true;
    }
}
