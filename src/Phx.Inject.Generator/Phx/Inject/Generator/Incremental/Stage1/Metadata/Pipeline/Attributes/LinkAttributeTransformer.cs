// -----------------------------------------------------------------------------
// <copyright file="LinkAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Link attribute data into metadata.
/// </summary>
/// <remarks>
///     <para><b>Purpose - Dependency Type Aliasing:</b></para>
///     <para>
///     [Link(typeof(TInput), typeof(TOutput))] creates type aliases that allow a dependency of one type
///     to satisfy requests for another type. Links enable interface-to-implementation mapping,
///     covariant/contravariant type substitution, and adapter patterns without manual factory methods.
///     The transformer extracts both input and output types plus optional label/qualifier pairs.
///     </para>
///     
///     <para><b>User Code Pattern - Type Aliases:</b></para>
///     <code>
///     [Link(typeof(UserService), typeof(IUserService))]
///     [Link(typeof(IUserService), typeof(IService))]
///     [Specification]
///     public interface IServices {
///         [Factory] UserService CreateUserService();
///         
///         // IUserService and IService are auto-satisfied via links
///         void ProcessRequest(IService service);
///     }
///     </code>
///     <para>
///     When ProcessRequest requests IService, generator follows the link chain:
///     IService ← IUserService ← UserService (factory exists), automatically wiring dependencies.
///     </para>
///     
///     <para><b>User Code Pattern - Qualified Links:</b></para>
///     <code>
///     [Link(typeof(ReadOnlyCache), typeof(ICache),
///           OutputLabel = "ReadOnly")]
///     [Link(typeof(WritableCache), typeof(ICache),
///           OutputLabel = "Writable")]
///     [Specification]
///     public interface ICaches {
///         [Factory] ReadOnlyCache CreateReadOnlyCache();
///         [Factory] WritableCache CreateWritableCache();
///         
///         void Process([Label("ReadOnly")] ICache readCache,
///                      [Label("Writable")] ICache writeCache);
///     }
///     </code>
///     <para>
///     Qualified links allow multiple links with same output type but different qualifiers,
///     enabling fine-grained dependency discrimination.
///     </para>
///     
///     <para><b>IAttributeListTransformer - Multiple Links Required:</b></para>
///     <para>
///     LinkAttributeTransformer implements IAttributeListTransformer rather than IAttributeTransformer
///     because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Specifications commonly need multiple links (interface hierarchies, multiple implementations)
///             </description>
///         </item>
///         <item>
///             <description>
///             Link chains require multiple link declarations (A→B, B→C, C→D)
///             </description>
///         </item>
///         <item>
///             <description>
///             AttributeUsage on LinkAttribute specifies AllowMultiple = true
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Constructor and Named Arguments - Dual Extraction Pattern:</b></para>
///     <para>
///     Link extraction handles both constructor and named arguments:
///     </para>
///     <list type="number">
///         <item>
///             <term>Constructor Arguments (Required):</term>
///             <description>
///             `GetConstructorArguments&lt;ITypeSymbol&gt;()` extracts input and output types (index 0 and 1).
///             These are positional and always present per attribute signature.
///             </description>
///         </item>
///         <item>
///             <term>Named Arguments (Optional):</term>
///             <description>
///             `GetNamedArgument&lt;T&gt;(propertyName)` extracts optional InputLabel, InputQualifier,
///             OutputLabel, OutputQualifier. Named arguments support optional qualification without
///             complicating constructor signature.
///             </description>
///         </item>
///     </list>
///     <para>
///     All ITypeSymbol values are converted to TypeModel via `.ToTypeModel()` for caching stability.
///     Nullable types (InputQualifier?, OutputQualifier?) correctly represent optional qualifier parameters.
///     </para>
///     
///     <para><b>Why Links Need Special Handling - Graph Resolution:</b></para>
///     <para>
///     Links create a dependency graph that must be resolved transitively:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             A→B, B→C link chain means requesting C can be satisfied by A factory
///             </description>
///         </item>
///         <item>
///             <description>
///             Qualified links create parallel graphs (labeled A→B exists separately from unlabeled A→B)
///             </description>
///         </item>
///         <item>
///             <description>
///             Circular links (A→B→A) must be detected to prevent infinite generation loops
///             </description>
///         </item>
///         <item>
///             <description>
///             Link metadata must be efficiently comparable for incremental caching of entire graphs
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Validation Constraints - Enforced by Later Stages:</b></para>
///     <para>
///     Transformer doesn't validate link semantics. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             No circular link chains (A→B→C→A causes infinite recursion)
///             </description>
///         </item>
///         <item>
///             <description>
///             InputLabel and InputQualifier aren't both specified (mutually exclusive)
///             </description>
///         </item>
///         <item>
///             <description>
///             OutputLabel and OutputQualifier aren't both specified (mutually exclusive)
///             </description>
///         </item>
///         <item>
///             <description>
///             Input type is assignment-compatible with output type (covariance rules)
///             </description>
///         </item>
///         <item>
///             <description>
///             Qualified links have consistent qualifier usage (if factory is labeled, link input must be labeled)
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Common Errors Prevented:</b></para>
///     <list type="bullet">
///         <item>
///             <term>Circular link chain:</term>
///             <description>
///             Validator detects A→B→C→A patterns that would cause infinite generation loops.
///             </description>
///         </item>
///         <item>
///             <term>Incompatible type assignment:</term>
///             <description>
///             Linking string to IUserService is caught (string doesn't implement IUserService).
///             </description>
///         </item>
///         <item>
///             <term>Qualifier mismatch:</term>
///             <description>
///             Link with OutputLabel but factory is unlabeled causes ambiguity, flagged by validator.
///             </description>
///         </item>
///         <item>
///             <term>Conflicting qualifiers:</term>
///             <description>
///             Setting both InputLabel and InputQualifier is ambiguous, rejected at validation.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal class LinkAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeListTransformer<LinkAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static LinkAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, LinkAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public EquatableList<LinkAttributeMetadata> Transform(ISymbol targetSymbol) {
        return attributeMetadataTransformer.GetAttributes(
            targetSymbol,
            LinkAttributeMetadata.AttributeClassName
        ).Select(metadata => {
            var (attributeData, attributeMetadata) = metadata;
            var constructorArgs = attributeData
                .GetConstructorArguments<ITypeSymbol>(argument => argument.Kind != TypedConstantKind.Array)
                .ToList();
            var input = constructorArgs[0].ToTypeModel();
            var output = constructorArgs[1].ToTypeModel();

            var inputLabel = attributeData.GetNamedArgument<string>(nameof(LinkAttribute.InputLabel));
            var inputQualifier = attributeData.GetNamedArgument<ITypeSymbol>(nameof(LinkAttribute.InputQualifier))?.ToTypeModel();
            var outputLabel = attributeData.GetNamedArgument<string>(nameof(LinkAttribute.OutputLabel));
            var outputQualifier = attributeData.GetNamedArgument<ITypeSymbol>(nameof(LinkAttribute.OutputQualifier))?.ToTypeModel();

            return new LinkAttributeMetadata(
                input,
                output,
                inputLabel,
                inputQualifier,
                outputLabel,
                outputQualifier,
                attributeMetadata);
        })
        .ToEquatableList();
    }
}
