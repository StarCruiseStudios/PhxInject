// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal class InjectorDependencyAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<InjectorDependencyAttributeMetadata>, IAttributeChecker {
    public static InjectorDependencyAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, InjectorDependencyAttributeMetadata.AttributeClassName);
    }

    public InjectorDependencyAttributeMetadata Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.SingleAttributeOrNull(
            targetSymbol,
            InjectorDependencyAttributeMetadata.AttributeClassName
        ) ?? throw new InvalidOperationException($"Expected single {InjectorDependencyAttributeMetadata.AttributeClassName} attribute on {targetSymbol.Name}");
        
        return new InjectorDependencyAttributeMetadata(attributeMetadata);
    }
}
