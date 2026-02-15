// -----------------------------------------------------------------------------
// <copyright file="QualifierAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms Qualifier attribute data into metadata.
/// </summary>
/// <remarks>
///     <para>Purpose - Type-Based Dependency Discrimination:</para>
///     <para>
///     [Qualifier(typeof(TMarker))] provides type-based discrimination for dependencies of the same type.
///     Unlike LabelAttribute which uses strings, QualifierAttribute uses marker types to distinguish
///     dependencies. The transformer extracts the qualifier type from the attribute's constructor argument
///     and converts it to a TypeModel for later comparison.
///     </para>
///     
///     <para>User Code Pattern - Type-Qualified Dependencies:</para>
///     <code>
///     public interface IApplicationMarker { }
///     public interface IPerformanceMarker { }
///     
///     [Specification]
///     public interface ILoggers {
///         [Factory, Qualifier(typeof(IApplicationMarker))]
///         ILogger CreateAppLogger();
///         
///         [Factory, Qualifier(typeof(IPerformanceMarker))]
///         ILogger CreatePerfLogger();
///         
///         void ProcessData(
///             [Qualifier(typeof(IApplicationMarker))] ILogger appLogger,
///             [Qualifier(typeof(IPerformanceMarker))] ILogger perfLogger);
///     }
///     </code>
///     <para>
///     Generator matches parameters with qualified factory methods by comparing qualifier types.
///     Type-based discrimination is compile-time safe (refactoring support, no typos) compared to strings.
///     </para>
///     
///     <para>Type Extraction - ITypeSymbol to TypeModel Conversion:</para>
///     <para>
///     Qualifier type extraction follows a two-step process:
///     </para>
///     <list type="number">
///         <item>
///             <description>
///             Extract ITypeSymbol: `GetConstructorArgument&lt;ITypeSymbol&gt;(argument => argument.Kind != TypedConstantKind.Array)`
///             filters for non-array type arguments.
///             </description>
///         </item>
///         <item>
///             <description>
///             Convert to TypeModel: `.ToTypeModel()` creates an equatable type representation that
///             works with incremental caching and doesn't hold Roslyn symbol references.
///             </description>
///         </item>
///     </list>
///     <para>
///     The null-forgiving operator (!) is safe because QualifierAttribute constructor requires a
///     non-null Type parameter.
///     </para>
///     
///     <para>Why Qualifiers Need Special Handling - Type Comparison Complexity:</para>
///     <para>
///     Qualifier types require TypeModel conversion instead of using ITypeSymbol directly because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             ITypeSymbol instances aren't stable across compilation passes (breaks incremental caching)
///             </description>
///         </item>
///         <item>
///             <description>
///             TypeModel provides structural equality (same type = equal) without reference equality
///             </description>
///         </item>
///         <item>
///             <description>
///             TypeModel is serializable for metadata caching between generator invocations
///             </description>
///         </item>
///         <item>
///             <description>
///             Prevents memory leaks from holding Roslyn compilation references
///             </description>
///         </item>
///     </list>
///     
///     <para>Qualifiers vs Labels - When To Use Each:</para>
///     <para>
///     Prefer Qualifiers when:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Want compile-time safety (rename refactoring, typo prevention)
///             </description>
///         </item>
///         <item>
///             <description>
///             Qualifier represents a semantic concept worth making explicit (IReadOnlyCache vs IWritableCache)
///             </description>
///         </item>
///         <item>
///             <description>
///             Multiple specifications share qualifier types (consistent qualification across modules)
///             </description>
///         </item>
///     </list>
///     <para>
///     Prefer Labels when discrimination is ad-hoc or conceptual rather than architectural.
///     </para>
///     
///     <para>Validation Constraints - Enforced by Later Stages:</para>
///     <para>
///     Transformer doesn't validate qualifier usage. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Qualifier type is accessible (not private or internal when used cross-assembly)
///             </description>
///         </item>
///         <item>
///             <description>
///             Qualifier uniqueness within a specification (no duplicate qualifiers for same type)
///             </description>
///         </item>
///         <item>
///             <description>
///             Parameter qualifiers match factory qualifiers (consistent qualification)
///             </description>
///         </item>
///         <item>
///             <description>
///             Label and Qualifier aren't used simultaneously (mutually exclusive)
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Wrong qualifier type:</term>
///             <description>
///             Compile-time error from typeof() prevents typos that would be possible with strings.
///             </description>
///         </item>
///         <item>
///             <term>Missing qualifier on parameter:</term>
///             <description>
///             When multiple qualified factories exist for a type, unqualified parameters are
///             flagged as ambiguous.
///             </description>
///         </item>
///         <item>
///             <term>Mixed Label and Qualifier:</term>
///             <description>
///             Validator rejects using both [Label] and [Qualifier] on same element (ambiguous intent).
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class QualifierAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<QualifierAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static QualifierAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, QualifierAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<QualifierAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            QualifierAttributeMetadata.AttributeClassName
        );

        var qualifierType = attributeData.GetConstructorArgument<ITypeSymbol>(
            argument => argument.Kind != TypedConstantKind.Array
        )!.ToTypeModel();

        return new QualifierAttributeMetadata(qualifierType, attributeMetadata).ToOkResult();
    }
}
