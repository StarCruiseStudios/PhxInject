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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal record AttributeMetadataPair(
    AttributeData Data,
    AttributeMetadata Metadata
) {
    public static AttributeMetadataPair From(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadataPair(attributeData, AttributeMetadata.Create(targetSymbol, attributeData));
    }
}

internal interface IAttributeMetadataTransformer {
    bool HasAttribute(ISymbol targetSymbol, string attributeClassName);
    EquatableList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        string attributeClassName);
    AttributeMetadataPair? SingleAttributeOrNull(
        ISymbol targetSymbol,
        string attributeClassName);
    AttributeMetadataPair ExpectSingleAttribute(
        ISymbol targetSymbol,
        string attributeClassName);
}

internal class AttributeMetadataTransformer : IAttributeMetadataTransformer {
    public static readonly AttributeMetadataTransformer Instance = new();
    
    public bool HasAttribute(ISymbol targetSymbol, string attributeClassName) {
        return targetSymbol.GetAttributes().Any(a => a.GetFullyQualifiedName() == attributeClassName);
    }

    public EquatableList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        return targetSymbol.GetAttributes()
            .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
            .Select(attributeData => AttributeMetadataPair.From(targetSymbol, attributeData))
            .ToEquatableList();
    }

    public AttributeMetadataPair? SingleAttributeOrNull(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        var attributeData = targetSymbol.GetAttributes()
            .SingleOrDefault(attributeData => attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName);
        return attributeData != null ? AttributeMetadataPair.From(targetSymbol, attributeData) : null;
    }
    
    public AttributeMetadataPair ExpectSingleAttribute(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        var attributeData = targetSymbol.GetAttributes()
            .SingleOrDefault(attributeData => attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName);
        return attributeData != null 
            ? AttributeMetadataPair.From(targetSymbol, attributeData) 
            : throw new InvalidOperationException(
                $"Expected single {attributeClassName} attribute on {targetSymbol.Name}. " +
                $"Found {targetSymbol.GetAttributes().Count(d => d.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName)}");
    }
    
    

}