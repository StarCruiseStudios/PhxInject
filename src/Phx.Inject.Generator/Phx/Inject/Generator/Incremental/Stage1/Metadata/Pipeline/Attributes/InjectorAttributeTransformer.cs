// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal class InjectorAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<InjectorAttributeMetadata>, IAttributeChecker {
    public static InjectorAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, InjectorAttributeMetadata.AttributeClassName);
    }

    public InjectorAttributeMetadata Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.SingleAttributeOrNull(
            targetSymbol,
            InjectorAttributeMetadata.AttributeClassName
        ) ?? throw new InvalidOperationException($"Expected single {InjectorAttributeMetadata.AttributeClassName} attribute on {targetSymbol.Name}");
        
        var generatedClassName = attributeData.GetNamedArgument<string>(nameof(InjectorAttribute.GeneratedClassName))
                                 ?? attributeData.GetConstructorArgument<string>(argument => argument.Kind != TypedConstantKind.Array);
            
        var specifications = attributeData
            .GetConstructorArguments<ITypeSymbol>(argument => argument.Kind != TypedConstantKind.Array)
            .Select(it => it.ToTypeModel())
            .ToImmutableList();
        
        return new InjectorAttributeMetadata(
            generatedClassName,
            specifications,
            attributeMetadata);
    }
}
