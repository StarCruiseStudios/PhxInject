// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

internal class LabelAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<LabelAttributeMetadata> {
    public static LabelAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public LabelAttributeMetadata Transform(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    ) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            attributes,
            LabelAttributeMetadata.AttributeClassName
        );

        var label = attributeData.GetConstructorArgument<string>(
            argument => argument.Kind != TypedConstantKind.Array
        )!;

        return new LabelAttributeMetadata(label, attributeMetadata);
    }
}
