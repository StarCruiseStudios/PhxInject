// -----------------------------------------------------------------------------
// <copyright file="AttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

internal record AttributeMetadataPair(
    AttributeData Data,
    AttributeMetadata Metadata
) {
    public static AttributeMetadataPair From(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadataPair(attributeData, AttributeMetadata.Create(targetSymbol, attributeData));
    }
}

internal interface IAttributeMetadataTransformer {
    bool HasAttribute(IEnumerable<AttributeData> attributes, string attributeClassName);
    IReadOnlyList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes,
        string attributeClassName);
    AttributeMetadataPair ExpectSingleAttribute(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes,
        string attributeClassName);
}

internal class AttributeMetadataTransformer : IAttributeMetadataTransformer {
    public static readonly AttributeMetadataTransformer Instance = new();
    
    public bool HasAttribute(IEnumerable<AttributeData> attributes, string attributeClassName) {
        return attributes.Any(a => a.GetFullyQualifiedName() == attributeClassName);
    }

    public IReadOnlyList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes,
        string attributeClassName
    ) {
        return attributes.Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
            .Select(attributeData => AttributeMetadataPair.From(targetSymbol, attributeData))
            .ToImmutableList();
    }

    public AttributeMetadataPair ExpectSingleAttribute(
        ISymbol targetSymbol,
        IEnumerable<AttributeData> attributes,
        string attributeClassName
    ) {
        var attributeData = attributes
            .Single(attributeData => attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName);
        return AttributeMetadataPair.From(targetSymbol, attributeData);
    }
}