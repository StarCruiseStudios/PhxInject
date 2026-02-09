// -----------------------------------------------------------------------------
// <copyright file="QualifierAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal class QualifierAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<QualifierAttributeMetadata>, IAttributeChecker {
    public static QualifierAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, QualifierAttributeMetadata.AttributeClassName);
    }

    public QualifierAttributeMetadata Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.SingleAttributeOrNull(
            targetSymbol,
            QualifierAttributeMetadata.AttributeClassName
        ) ?? throw new InvalidOperationException($"Expected single {QualifierAttributeMetadata.AttributeClassName} attribute on {targetSymbol.Name}");

        var qualifierType = attributeData.GetConstructorArgument<ITypeSymbol>(
            argument => argument.Kind != TypedConstantKind.Array
        )!.ToTypeModel();

        return new QualifierAttributeMetadata(qualifierType, attributeMetadata);
    }
}
