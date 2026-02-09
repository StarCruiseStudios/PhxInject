// -----------------------------------------------------------------------------
// <copyright file="SpecificationAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal class SpecificationAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<SpecificationAttributeMetadata>, IAttributeChecker {
    public static SpecificationAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, SpecificationAttributeMetadata.AttributeClassName);
    }

    public SpecificationAttributeMetadata Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.SingleAttributeOrNull(
            targetSymbol,
            SpecificationAttributeMetadata.AttributeClassName
        ) ?? throw new InvalidOperationException($"Expected single {SpecificationAttributeMetadata.AttributeClassName} attribute on {targetSymbol.Name}");
        
        return new SpecificationAttributeMetadata(attributeMetadata);
    }
}
