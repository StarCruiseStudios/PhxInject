// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

internal class DependencyAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<DependencyAttributeMetadata>, IAttributeChecker {
    public static DependencyAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, DependencyAttributeMetadata.AttributeClassName);
    }

    public DependencyAttributeMetadata Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.SingleAttributeOrNull(
            targetSymbol,
            DependencyAttributeMetadata.AttributeClassName
        ) ?? throw new InvalidOperationException($"Expected single {DependencyAttributeMetadata.AttributeClassName} attribute on {targetSymbol.Name}");

        var dependencyType = attributeData.GetConstructorArgument<ITypeSymbol>(
            argument => argument.Kind != TypedConstantKind.Array
        )!.ToTypeModel();

        return new DependencyAttributeMetadata(dependencyType, attributeMetadata);
    }
}
