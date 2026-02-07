// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

internal class AutoBuilderAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<AutoBuilderAttributeMetadata>, IAttributeChecker {
    public static AutoBuilderAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, AutoBuilderAttributeMetadata.AttributeClassName);
    }

    public AutoBuilderAttributeMetadata Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.SingleAttributeOrNull(
            targetSymbol,
            AutoBuilderAttributeMetadata.AttributeClassName
        ) ?? throw new InvalidOperationException($"Expected single {AutoBuilderAttributeMetadata.AttributeClassName} attribute on {targetSymbol.Name}");
        
        return new AutoBuilderAttributeMetadata(attributeMetadata);
    }
}
