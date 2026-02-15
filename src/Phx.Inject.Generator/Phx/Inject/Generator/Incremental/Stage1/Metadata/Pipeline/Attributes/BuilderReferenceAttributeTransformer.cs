// -----------------------------------------------------------------------------
// <copyright file="BuilderReferenceAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms BuilderReference attribute data into metadata.
/// </summary>
/// <remarks>
///     Marker attribute for parameters that receive externally-configured builder instances (caller
///     controls builder state). Prevents generator from injecting builder as dependency. Generated
///     method signature exposes builder parameter. Mutually exclusive with <c>[FactoryReference]</c>
///     and dependency qualifiers. Inverse of <c>[AutoBuilder]</c> (internal generation).
/// </remarks>
internal sealed class BuilderReferenceAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<BuilderReferenceAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static BuilderReferenceAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, BuilderReferenceAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<BuilderReferenceAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            BuilderReferenceAttributeMetadata.AttributeClassName
        );
        
        return new BuilderReferenceAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
