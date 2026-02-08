// -----------------------------------------------------------------------------
// <copyright file="FieldElementValidator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Validators;

internal class FieldElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? isStatic = null,
    bool? isReadonly = null,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null,
    IReadOnlyList<IAttributeChecker>? prohibitedAttributes = null
) : ICodeElementValidator {
    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;
    private readonly IReadOnlyList<IAttributeChecker> prohibitedAttributes = prohibitedAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    public bool IsValidSymbol(ISymbol symbol) {
        if (requiredAttributes.Any(requiredAttribute => !requiredAttribute.HasAttribute(symbol))) {
            return false;
        }
        
        if (prohibitedAttributes.Any(prohibitedAttribute => prohibitedAttribute.HasAttribute(symbol))) {
            return false;
        }

        if (symbol is not IFieldSymbol fieldSymbol) {
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

    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        if (syntaxNode is not FieldDeclarationSyntax) {
            return false;
        }

        return true;
    }
}
