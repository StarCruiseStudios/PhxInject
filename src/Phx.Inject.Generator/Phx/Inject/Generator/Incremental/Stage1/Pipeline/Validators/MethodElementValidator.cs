// -----------------------------------------------------------------------------
// <copyright file="MethodElementValidator.cs" company="Star Cruise Studios LLC">
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

internal class MethodElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    bool? returnsVoid = null,
    int? minParameterCount = null,
    int? maxParameterCount = null,
    bool? isStatic = null,
    bool? isAbstract = null,
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

        if (symbol is not IMethodSymbol methodSymbol) {
            return false;
        }

        if (!requiredAccessibility.AccessibilityMatches(methodSymbol.DeclaredAccessibility)) {
            return false;
        }

        if (returnsVoid != null && returnsVoid != methodSymbol.ReturnsVoid) {
            return false;
        }

        if (minParameterCount != null && minParameterCount.Value > methodSymbol.Parameters.Length) {
            return false;
        }
        
        if (maxParameterCount != null && maxParameterCount.Value < methodSymbol.Parameters.Length) {
            return false;
        }

        if (isStatic != null && isStatic != methodSymbol.IsStatic) {
            return false;
        }

        if (isAbstract != null) {
            var methodIsAbstract = methodSymbol.IsAbstract || methodSymbol.ContainingType.TypeKind == TypeKind.Interface;
            if (isAbstract != methodIsAbstract) {
                return false;
            }
        }

        return true;
    }

    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        if (syntaxNode is not MethodDeclarationSyntax methodDeclaration) {
            return false;
        }

        // Note: requiredAttributes check cannot be performed on syntax nodes
        // as attributes are not fully resolved at this stage

        var modifiers = methodDeclaration.Modifiers;

        switch (requiredAccessibility) {
            case CodeElementAccessibility.PublicOrInternal:
                // Must have at least one public/internal modifier and no private/protected
                if (!modifiers.Any(modifier => modifier.ValueText is "internal" or "public")) {
                    return false;
                }
                if (modifiers.Any(modifier => modifier.ValueText is "private" or "protected")) {
                    return false;
                }
                break;
            case CodeElementAccessibility.Any:
                break;
            default:
                throw new InvalidOperationException($"Unknown accessibility value: {requiredAccessibility}.");
        }

        if (returnsVoid != null) {
            var hasVoidReturnType = methodDeclaration.ReturnType.ToString() == "void";
            if (returnsVoid != hasVoidReturnType) {
                return false;
            }
        }

        if (minParameterCount != null && minParameterCount.Value > methodDeclaration.ParameterList.Parameters.Count) {
            return false;
        }

        if (maxParameterCount != null && maxParameterCount.Value < methodDeclaration.ParameterList.Parameters.Count) {
            return false;
        }

        if (isStatic != null) {
            var hasStaticModifier = modifiers.Any(modifier => modifier.ValueText == "static");
            if (isStatic != hasStaticModifier) {
                return false;
            }
        }

        if (isAbstract != null) {
            var hasAbstractModifier = modifiers.Any(modifier => modifier.ValueText == "abstract");
            // Note: Cannot fully determine if containing type is interface from syntax alone
            // The abstract modifier check will catch most cases
            if (isAbstract != hasAbstractModifier) {
                return false;
            }
        }

        return true;
    }
}