// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

internal interface IAttributeTransformer<out TAttributeMetadata> where TAttributeMetadata : IAttributeElement {
    TAttributeMetadata Transform(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    );
}

internal class PhxInjectAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<PhxInjectAttributeMetadata> {
    public static PhxInjectAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public PhxInjectAttributeMetadata Transform(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    ) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            attributes,
            PhxInjectAttributeMetadata.AttributeClassName
        );
        
        return new PhxInjectAttributeMetadata(
            attributeData.GetNamedIntArgument(nameof(PhxInjectAttribute.TabSize)),
            attributeData.GetNamedStringArgument(nameof(PhxInjectAttribute.GeneratedFileExtension)),
            attributeData.GetNamedBoolArgument(nameof(PhxInjectAttribute.NullableEnabled)),
            attributeMetadata);
    }
}
