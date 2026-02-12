// -----------------------------------------------------------------------------
// <copyright file="BuilderAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Builder attribute data into metadata.
/// </summary>
internal class BuilderAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<BuilderAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static BuilderAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, BuilderAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<BuilderAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            BuilderAttributeMetadata.AttributeClassName
        );
        
        return new BuilderAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
