// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Injector attribute data into metadata.
/// </summary>
/// <remarks>
///     <para>Purpose - Root Injector Configuration:</para>
///     <para>
///     [Injector] marks interfaces or classes that should have generated dependency injector
///     implementations. This is the entry point attribute that triggers the entire code generation
///     pipeline. The transformer extracts configuration controlling how the injector is generated.
///     </para>
///     
///     <para>Attribute Arguments - What We Extract and WHY:</para>
///     <list type="number">
///         <item>
///             <term>GeneratedClassName (string, optional):</term>
///             <description>
///             The name for the generated implementation class. Extracted from either named argument
///             or first non-array constructor argument. Users specify this to avoid name collisions
///             or follow naming conventions (e.g., "ProdAppInjector" vs "TestAppInjector").
///             If null, generator falls back to convention-based naming (interface name + "Impl").
///             </description>
///         </item>
///         <item>
///             <term>Specifications (ITypeSymbol[], optional):</term>
///             <description>
///             Additional specification interfaces this injector should implement. Extracted by
///             filtering constructor arguments for non-array TypedConstant entries (distinguishes
///             from other type arguments). Specifications define contracts for provided dependencies
///             without requiring the interface to explicitly declare all provider methods.
///             WHY: Allows injector to satisfy multiple interface contracts, enabling composition
///             patterns where different components expect different subsets of dependencies.
///             </description>
///         </item>
///     </list>
///     
///     <para>Argument Extraction Strategy - Named vs Positional:</para>
///     <para>
///     Checks named arguments first (GetNamedArgument), falls back to constructor arguments
///     (GetConstructorArgument). This dual-check pattern handles both attribute invocation styles:
///     </para>
///     <code>
///     [Injector(GeneratedClassName = "MyInjector")]  // Named argument
///     [Injector("MyInjector")]                        // Positional argument
///     </code>
///     <para>
///     Named arguments are preferred (checked first) as they're more explicit and version-resilient.
///     Constructor argument extraction requires predicates since parameter names are unavailable -
///     we use `argument.Kind != TypedConstantKind.Array` to filter for the string argument vs
///     the Type[] specification array.
///     </para>
///     
///     <para>Type Argument Handling - ITypeSymbol to TypeModel:</para>
///     <para>
///     Specification types are extracted as ITypeSymbol from Roslyn, then converted to TypeModel
///     via ToTypeModel(). This conversion is crucial:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             ITypeSymbol is Roslyn's semantic representation, tied to Compilation lifetime
///             </description>
///         </item>
///         <item>
///             <description>
///             TypeModel is our domain representation with structural equality for incremental caching
///             </description>
///         </item>
///         <item>
///             <description>
///             TypeModel captures fully qualified names and generic arity without holding Compilation references
///             </description>
///         </item>
///     </list>
///     
///     <para>Error Handling - Why No Validation Here:</para>
///     <para>
///     This transformer performs no semantic validation (e.g., checking if GeneratedClassName is
///     a valid identifier, or if specifications are actually interfaces). Returns ToOkResult()
///     unconditionally. WHY:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Attribute extraction phase should be pure data transformation
///             </description>
///         </item>
///         <item>
///             <description>
///             Validation happens in dedicated validator pipeline after all attributes are extracted
///             </description>
///         </item>
///         <item>
///             <description>
///             Allows validators to see complete attribute context before reporting errors
///             </description>
///         </item>
///     </list>
///     <para>
///     If attribute arguments are fundamentally malformed (e.g., wrong types), Roslyn's
///     GetConstructorArgument extensions return null/default, which later validators can detect.
///     </para>
///     
///     <para>Performance Considerations:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Only called when HasAttribute confirmed [Injector] presence (avoid redundant GetAttributes)
///             </description>
///         </item>
///         <item>
///             <description>
///             ExpectSingleAttribute uses SingleOrDefault, catching attribute duplication early
///             </description>
///         </item>
///         <item>
///             <description>
///             ToTypeModel() allocates TypeModel but is unavoidable for incremental caching contract
///             </description>
///         </item>
///     </list>
/// </remarks>
internal class InjectorAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<InjectorAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static InjectorAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, InjectorAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<InjectorAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            InjectorAttributeMetadata.AttributeClassName
        );
        
        var generatedClassName = attributeData.GetNamedArgument<string>(nameof(InjectorAttribute.GeneratedClassName))
                                 ?? attributeData.GetConstructorArgument<string>(argument => argument.Kind != TypedConstantKind.Array);
            
        var specifications = attributeData
            .GetConstructorArguments<ITypeSymbol>(argument => argument.Kind != TypedConstantKind.Array)
            .Select(it => it.ToTypeModel())
            .ToEquatableList();
        
        return new InjectorAttributeMetadata(
            generatedClassName,
            specifications,
            attributeMetadata).ToOkResult();
    }
}
