// -----------------------------------------------------------------------------
// <copyright file="SpecificationAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

internal class SpecificationAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<SpecificationAttributeMetadata> {
    public static SpecificationAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public SpecificationAttributeMetadata Transform(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    ) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            attributes,
            SpecificationAttributeMetadata.AttributeClassName
        );
        
        return new SpecificationAttributeMetadata(attributeMetadata);
    }
}
