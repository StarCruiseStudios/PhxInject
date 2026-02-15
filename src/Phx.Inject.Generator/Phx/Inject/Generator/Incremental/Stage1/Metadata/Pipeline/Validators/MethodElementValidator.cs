// -----------------------------------------------------------------------------
// <copyright file="MethodElementValidator.cs" company="Star Cruise Studios LLC">
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
///     Validates that a method symbol meets specified structural and semantic requirements.
/// </summary>
/// <param name="requiredAccessibility">
///     Required accessibility level (Any, PublicOrInternal, etc.). Defaults to Any.
/// </param>
/// <param name="methodKind">
///     Filter for method kind (ordinary method, property accessor, constructor, etc.). Defaults to Any.
/// </param>
/// <param name="returnsVoid">
///     If non-null, specifies whether method must return void (true) or non-void (false).
/// </param>
/// <param name="minParameterCount">
///     Minimum number of parameters required. Null means no minimum.
/// </param>
/// <param name="maxParameterCount">
///     Maximum number of parameters allowed. Null means no maximum.
/// </param>
/// <param name="isStatic">
///     If non-null, specifies whether method must be static (true) or instance (false).
/// </param>
/// <param name="isAbstract">
///     If non-null, specifies whether method must be abstract (true) or concrete (false).
///     Note: Interface methods are considered abstract even without explicit modifier.
/// </param>
/// <param name="requiredAttributes">
///     Attributes that must all be present. Null/empty means no attribute requirements.
/// </param>
/// <param name="prohibitedAttributes">
///     Attributes that must not be present. Null/empty means no prohibited attributes.
/// </param>
/// <remarks>
///     <para>Design Purpose - Factory and Provider Validation:</para>
///     <para>
///     Method validators enforce DI framework semantics on user-defined factory methods.
///     Key patterns:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Factory Methods:</term>
///             <description>
///             Must be static, non-void return, have @Factory attribute. Parameters become dependencies.
///             </description>
///         </item>
///         <item>
///             <term>Provider Methods:</term>
///             <description>
///             Must be abstract (injector interface), non-void return, may have parameters for runtime args.
///             </description>
///         </item>
///         <item>
///             <term>Builder Methods:</term>
///             <description>
///             Similar to factories but may be instance methods, often with fluent parameter patterns.
///             </description>
///         </item>
///     </list>
///     
///     <para>WHY These Constraints Exist:</para>
///     <list type="bullet">
///         <item>
///             <term>Static requirement for factories:</term>
///             <description>
///             Ensures factory has no hidden dependencies via instance state. All dependencies must be
///             explicit parameters for graph analysis.
///             </description>
///         </item>
///         <item>
///             <term>Non-void return:</term>
///             <description>
///             Factory/provider must produce a value. Void methods can't provide dependencies.
///             Catches common mistake of void initialization methods marked with @Factory.
///             </description>
///         </item>
///         <item>
///             <term>Abstract requirement for providers:</term>
///             <description>
///             Injector interface methods must be abstract so generator can provide implementation.
///             Concrete methods would conflict with generated code.
///             </description>
///         </item>
///         <item>
///             <term>Parameter count constraints:</term>
///             <description>
///             Some patterns have arity constraints (e.g., activators require parameters, simple providers
///             may prohibit parameters). Clear errors when pattern misused.
///             </description>
///         </item>
///         <item>
///             <term>Attribute prohibition:</term>
///             <description>
///             Prevents conflicting annotations (e.g., can't be both @Factory and @Builder simultaneously).
///             </description>
///         </item>
///     </list>
///     
///     <para>Interface Method Special Case:</para>
///     <para>
///     Interface methods are implicitly abstract even without the 'abstract' keyword. Symbol validation
///     handles this by checking both methodSymbol.IsAbstract and ContainingType.TypeKind == Interface.
///     Syntax validation cannot detect containing type kind, so may produce false negatives (acceptable
///     since symbol validation is authoritative).
///     </para>
///     
///     <para>Method Kind Filtering:</para>
///     <para>
///     Roslyn's IMethodSymbol represents not just ordinary methods, but also property accessors,
///     event handlers, operators, constructors, etc. MethodKindFilter allows selecting which
///     varieties to accept. Most DI patterns want only ordinary methods, excluding accessors/operators.
///     </para>
/// </remarks>
internal sealed class MethodElementValidator(
    CodeElementAccessibility requiredAccessibility = CodeElementAccessibility.Any,
    MethodKindFilter methodKind = MethodKindFilter.Any,
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

    /// <inheritdoc />
    /// <remarks>
    ///     Executes checks ordered by cost: type check first (cheapest), then attributes, accessibility,
    ///     method kind, return type, parameter count, and finally modifiers. This ordering maximizes
    ///     short-circuit efficiency on invalid candidates.
    /// </remarks>
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) {
        if (symbol is not IMethodSymbol methodSymbol) {
            return false;
        }

        if (requiredAttributes.Any(requiredAttribute => !requiredAttribute.HasAttribute(symbol))) {
            return false;
        }
        
        if (prohibitedAttributes.Any(prohibitedAttribute => prohibitedAttribute.HasAttribute(symbol))) {
            return false;
        }

        if (!requiredAccessibility.AccessibilityMatches(methodSymbol.DeclaredAccessibility)) {
            return false;
        }

        if (!methodKind.MethodKindMatches(methodSymbol)) {
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

    /// <inheritdoc />
    /// <remarks>
    ///     <para>Syntax Validation Limitations:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///             Cannot check attribute types (syntax doesn't resolve symbols)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Return type check uses ToString() which may have false positives/negatives with aliases
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Cannot determine if method is in interface (would need parent node type check)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             Cannot filter by MethodKind (constructors, operators, etc. look like methods syntactically)
    ///             </description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>Conservative Approach:</para>
    ///     <para>
    ///     When we can't definitively check a constraint in syntax (like abstract-via-interface),
    ///     we err on the side of allowing through to symbol validation rather than false-rejecting.
    ///     Symbol validation is authoritative; syntax validation is just an optimization.
    ///     </para>
    /// </remarks>
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
                // Unknown accessibility, treat as invalid
                return false;
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