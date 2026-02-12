// -----------------------------------------------------------------------------
// <copyright file="AttributeMetadataTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Pairs attribute data with its corresponding metadata.
/// </summary>
internal record AttributeMetadataPair(
    AttributeData Data,
    AttributeMetadata Metadata
) {
    /// <summary>
    ///     Creates an attribute metadata pair from a symbol and attribute data.
    /// </summary>
    /// <param name="targetSymbol">The symbol that has the attribute.</param>
    /// <param name="attributeData">The attribute data from Roslyn.</param>
    /// <returns>A new attribute metadata pair.</returns>
    public static AttributeMetadataPair From(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadataPair(attributeData, AttributeMetadata.Create(targetSymbol, attributeData));
    }
}

/// <summary>
///     Provides methods to transform and query attribute data.
/// </summary>
internal interface IAttributeMetadataTransformer {
    /// <summary>
    ///     Checks if the target symbol has the specified attribute.
    /// </summary>
    /// <param name="targetSymbol">The symbol to check.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>True if the attribute is present; otherwise, false.</returns>
    bool HasAttribute(ISymbol targetSymbol, string attributeClassName);
    
    /// <summary>
    ///     Gets all attributes of the specified type from the target symbol.
    /// </summary>
    /// <param name="targetSymbol">The symbol to query.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>A list of attribute metadata pairs.</returns>
    EquatableList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        string attributeClassName);
    
    /// <summary>
    ///     Gets a single attribute of the specified type, or null if not present.
    /// </summary>
    /// <param name="targetSymbol">The symbol to query.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>The attribute metadata pair, or null if not found.</returns>
    AttributeMetadataPair? SingleAttributeOrNull(
        ISymbol targetSymbol,
        string attributeClassName);
    
    /// <summary>
    ///     Gets a single attribute of the specified type, throwing if not present.
    /// </summary>
    /// <param name="targetSymbol">The symbol to query.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>The attribute metadata pair.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the attribute is not found.</exception>
    AttributeMetadataPair ExpectSingleAttribute(
        ISymbol targetSymbol,
        string attributeClassName);
}

/// <summary>
///     Transforms attribute data into attribute metadata.
/// </summary>
internal class AttributeMetadataTransformer : IAttributeMetadataTransformer {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly AttributeMetadataTransformer Instance = new();
    
    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol, string attributeClassName) {
        return targetSymbol.GetAttributes().Any(a => a.GetFullyQualifiedName() == attributeClassName);
    }

    /// <inheritdoc />
    public EquatableList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        return targetSymbol.GetAttributes()
            .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
            .Select(attributeData => AttributeMetadataPair.From(targetSymbol, attributeData))
            .ToEquatableList();
    }

    /// <inheritdoc />
    public AttributeMetadataPair? SingleAttributeOrNull(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        var attributeData = targetSymbol.GetAttributes()
            .SingleOrDefault(attributeData => attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName);
        return attributeData != null ? AttributeMetadataPair.From(targetSymbol, attributeData) : null;
    }
    
    /// <inheritdoc />
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