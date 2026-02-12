// -----------------------------------------------------------------------------
// <copyright file="LinkAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal class LinkAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeListTransformer<LinkAttributeMetadata> {
    public static LinkAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, LinkAttributeMetadata.AttributeClassName);
    }

    public EquatableList<LinkAttributeMetadata> Transform(ISymbol targetSymbol) {
        return attributeMetadataTransformer.GetAttributes(
            targetSymbol,
            LinkAttributeMetadata.AttributeClassName
        ).Select(metadata => {
            var (attributeData, attributeMetadata) = metadata;
            var constructorArgs = attributeData
                .GetConstructorArguments<ITypeSymbol>(argument => argument.Kind != TypedConstantKind.Array)
                .ToList();
            var input = constructorArgs[0].ToTypeModel();
            var output = constructorArgs[1].ToTypeModel();

            var inputLabel = attributeData.GetNamedArgument<string>(nameof(LinkAttribute.InputLabel));
            var inputQualifier = attributeData.GetNamedArgument<ITypeSymbol>(nameof(LinkAttribute.InputQualifier))?.ToTypeModel();
            var outputLabel = attributeData.GetNamedArgument<string>(nameof(LinkAttribute.OutputLabel));
            var outputQualifier = attributeData.GetNamedArgument<ITypeSymbol>(nameof(LinkAttribute.OutputQualifier))?.ToTypeModel();

            return new LinkAttributeMetadata(
                input,
                output,
                inputLabel,
                inputQualifier,
                outputLabel,
                outputQualifier,
                attributeMetadata);
        })
        .ToEquatableList();
    }
}
