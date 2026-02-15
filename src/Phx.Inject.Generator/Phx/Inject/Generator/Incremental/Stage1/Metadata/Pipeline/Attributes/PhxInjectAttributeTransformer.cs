// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms PhxInject attribute data into metadata.
/// </summary>
internal sealed class PhxInjectAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<PhxInjectAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static PhxInjectAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, PhxInjectAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<PhxInjectAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            PhxInjectAttributeMetadata.AttributeClassName
        );
        
        return new PhxInjectAttributeMetadata(
            attributeData.GetNamedIntArgument(nameof(PhxInjectAttribute.TabSize)),
            attributeData.GetNamedArgument<string>(nameof(PhxInjectAttribute.GeneratedFileExtension)),
            attributeData.GetNamedBoolArgument(nameof(PhxInjectAttribute.NullableEnabled)),
            attributeMetadata).ToOkResult();
    }
}
