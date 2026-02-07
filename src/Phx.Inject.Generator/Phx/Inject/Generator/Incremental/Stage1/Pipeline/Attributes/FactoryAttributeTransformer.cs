// -----------------------------------------------------------------------------
// <copyright file="FactoryAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Generator.Incremental.PhxInject;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;

internal class FactoryAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<FactoryAttributeMetadata>, IAttributeChecker {
    public static FactoryAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    private const string FabricationModeClassName = $"{NamespaceName}.{nameof(FabricationMode)}";

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, FactoryAttributeMetadata.AttributeClassName);
    }

    public FactoryAttributeMetadata Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.SingleAttributeOrNull(
            targetSymbol,
            FactoryAttributeMetadata.AttributeClassName
        ) ?? throw new InvalidOperationException($"Expected single {FactoryAttributeMetadata.AttributeClassName} attribute on {targetSymbol.Name}");

        var fabricationMode =
            attributeData.GetNamedArgument<FabricationMode?>(nameof(FactoryAttribute.FabricationMode))
            ?? attributeData.GetConstructorArgument<FabricationMode>(argument =>
                argument.Type!.GetFullyQualifiedName() == FabricationModeClassName,
                default);

        return new FactoryAttributeMetadata(fabricationMode, attributeMetadata);
    }
}
