// -----------------------------------------------------------------------------
// <copyright file="ChildInjectorAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms ChildInjector attribute data into metadata.
/// </summary>
internal class ChildInjectorAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<ChildInjectorAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static ChildInjectorAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, ChildInjectorAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<ChildInjectorAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            ChildInjectorAttributeMetadata.AttributeClassName
        );
        
        return new ChildInjectorAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
