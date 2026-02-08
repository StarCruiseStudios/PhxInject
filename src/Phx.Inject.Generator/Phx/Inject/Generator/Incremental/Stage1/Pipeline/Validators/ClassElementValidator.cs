// -----------------------------------------------------------------------------
// <copyright file="ClassElementValidator.cs" company="Star Cruise Studios LLC">
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

internal class ClassElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? isStatic = null,
    bool? isAbstract = null,
    IReadOnlyList<IAttributeChecker>? requiredAttributes = null
) : ICodeElementValidator {
    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    public bool IsValidSymbol(ISymbol symbol) {
        if (requiredAttributes.Any(requiredAttribute => !requiredAttribute.HasAttribute(symbol))) {
            return false;
        }

        if (symbol is not ITypeSymbol typeSymbol) {
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
                throw new InvalidOperationException($"Unknown accessibility value: {requiredAccessibility}.");
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