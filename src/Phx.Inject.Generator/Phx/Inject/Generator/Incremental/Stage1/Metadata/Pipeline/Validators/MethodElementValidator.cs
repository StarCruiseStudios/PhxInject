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
/// <param name="requiredAccessibility">Required accessibility level. Defaults to Any.</param>
/// <param name="methodKind">Filter for method kind (ordinary, accessor, etc.). Defaults to Any.</param>
/// <param name="returnsVoid">If non-null, specifies whether method must return void.</param>
/// <param name="minParameterCount">Minimum number of parameters required. Null means no minimum.</param>
/// <param name="maxParameterCount">Maximum number of parameters allowed. Null means no maximum.</param>
/// <param name="isStatic">If non-null, specifies whether method must be static.</param>
/// <param name="isAbstract">
///     If non-null, specifies whether method must be abstract.
///     Interface methods are considered abstract even without explicit modifier.
/// </param>
/// <param name="requiredAttributes">Attributes that must all be present.</param>
/// <param name="prohibitedAttributes">Attributes that must not be present.</param>
/// <remarks>
///     Enforces DI framework semantics for factory methods (static, non-void), provider methods
///     (abstract, non-void), and builder methods. Validates accessibility, parameter counts, and
///     attribute presence to prevent malformed dependency configurations.
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
    /// <summary>
    ///     Validates public/internal instance methods with no parameters that return non-void
    ///     (typical injector provider method).
    /// </summary>
    public static readonly MethodElementValidator InjectorProviderMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: false,
        maxParameterCount: 0,
        returnsVoid: false,
        prohibitedAttributes: ImmutableList.Create(ChildInjectorAttributeTransformer.Instance));
    
    /// <summary>
    ///     Validates public/internal instance methods with exactly one parameter that return void
    ///     and have the ChildInjector attribute (typical injector activator method).
    /// </summary>
    public static readonly MethodElementValidator InjectorActivatorMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: false,
        minParameterCount: 1,
        maxParameterCount: 1,
        returnsVoid: true,
        requiredAttributes: ImmutableList.Create(ChildInjectorAttributeTransformer.Instance));
    
    /// <summary>
    ///     Validates public/internal instance methods with no parameters that return non-void
    ///     and have the ChildInjector attribute (typical injector child provider method).
    /// </summary>
    public static readonly MethodElementValidator InjectorChildProviderMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: false,
        maxParameterCount: 0,
        returnsVoid: false,
        requiredAttributes: ImmutableList.Create(ChildInjectorAttributeTransformer.Instance));
    
    /// <summary>
    ///     Validates public/internal methods that return non-void with Factory attribute
    ///     (typical specification factory method).
    /// </summary>
    public static readonly MethodElementValidator SpecificationFactoryMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        returnsVoid: false,
        requiredAttributes: ImmutableList.Create<IAttributeChecker>(FactoryAttributeTransformer.Instance),
        prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(BuilderAttributeTransformer.Instance));
    
    /// <summary>
    ///     Validates public/internal static methods that return void with Builder attribute
    ///     (typical specification builder method).
    /// </summary>
    public static readonly MethodElementValidator SpecificationBuilderMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: true,
        returnsVoid: true,
        requiredAttributes: ImmutableList.Create<IAttributeChecker>(BuilderAttributeTransformer.Instance),
        prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(FactoryAttributeTransformer.Instance));
    
    /// <summary>
    ///     Validates public/internal static methods that return non-void with AutoBuilder attribute
    ///     (typical auto-builder method).
    /// </summary>
    public static readonly MethodElementValidator AutoBuilderMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: true,
        returnsVoid: false,
        requiredAttributes: ImmutableList.Create<IAttributeChecker>(AutoBuilderAttributeTransformer.Instance));
    
    /// <summary>
    ///     Validates public/internal instance methods that return non-void
    ///     (typical auto-factory constructor or factory method).
    /// </summary>
    public static readonly MethodElementValidator AutoFactoryMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: false,
        returnsVoid: false);
    
    /// <summary>
    ///     Validates public/internal abstract instance methods in pipelines.
    /// </summary>
    public static readonly MethodElementValidator AbstractPipelineMethod = new(
        requiredAccessibility: CodeElementAccessibility.PublicOrInternal,
        isStatic: false,
        isAbstract: true);

    private readonly IReadOnlyList<IAttributeChecker> requiredAttributes = requiredAttributes ?? ImmutableList<IAttributeChecker>.Empty;
    private readonly IReadOnlyList<IAttributeChecker> prohibitedAttributes = prohibitedAttributes ?? ImmutableList<IAttributeChecker>.Empty;

    /// <inheritdoc />
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