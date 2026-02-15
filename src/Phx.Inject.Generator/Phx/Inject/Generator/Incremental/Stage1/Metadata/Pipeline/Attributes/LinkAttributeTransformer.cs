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
///     Extracts type alias mappings from <c>[Link(typeof(TInput), typeof(TOutput))]</c> with optional
///     label/qualifier pairs. Constructor arguments (index 0/1) provide input/output types, named
///     arguments provide optional qualification. Implements <c>IAttributeListTransformer</c> for
///     multiple links per specification (interface hierarchies, link chains). All <c>ITypeSymbol</c>
///     values converted to <c>TypeModel</c> for cache stability.
/// </remarks>
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
/// </remarks>
internal sealed class LinkAttributeTransformer(
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
