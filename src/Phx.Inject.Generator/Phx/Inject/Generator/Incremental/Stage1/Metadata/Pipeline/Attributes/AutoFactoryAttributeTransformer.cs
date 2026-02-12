// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Generator.Incremental.PhxInject;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal class AutoFactoryAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<AutoFactoryAttributeMetadata>, IAttributeChecker {
    public static AutoFactoryAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    private const string FabricationModeClassName = $"{NamespaceName}.{nameof(FabricationMode)}";

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, AutoFactoryAttributeMetadata.AttributeClassName);
    }

    public IResult<AutoFactoryAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            AutoFactoryAttributeMetadata.AttributeClassName
        );

        var fabricationMode =
            attributeData.GetNamedArgument<FabricationMode?>(nameof(AutoFactoryAttribute.FabricationMode))
            ?? attributeData.GetConstructorArgument<FabricationMode>(argument =>
                argument.Type!.GetFullyQualifiedName() == FabricationModeClassName,
                default);

        return new AutoFactoryAttributeMetadata(fabricationMode, attributeMetadata).ToOkResult();
    }
}
