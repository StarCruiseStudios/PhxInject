// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms Label attribute data into metadata.
/// </summary>
/// <remarks>
///     Extracts string label from <c>[Label("name")]</c> for dependency discrimination when multiple
///     dependencies share the same type. Uses <c>GetConstructorArgument</c> with <c>Kind !=
///     TypedConstantKind.Array</c> filter to extract single string value. Null-forgiving operator safe
///     because <c>LabelAttribute</c> constructor requires non-null string parameter.
/// </remarks>
internal sealed class LabelAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<LabelAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static LabelAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, LabelAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<LabelAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            LabelAttributeMetadata.AttributeClassName
        );

        var label = attributeData.GetConstructorArgument<string>(
            argument => argument.Kind != TypedConstantKind.Array
        )!;

        return new LabelAttributeMetadata(label, attributeMetadata).ToOkResult();
    }
}
