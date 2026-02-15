// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Injector attribute data into metadata.
/// </summary>
/// <remarks>
///     Extracts configuration from <c>[Injector]</c> attributes that mark interfaces/classes for
///     generated DI implementations. Extracts GeneratedClassName (optional custom name) and
///     Specifications (additional interfaces to implement). Performs pure data transformation without
///     validation - validators check semantic correctness after extraction.
/// </remarks>
internal sealed class InjectorAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<InjectorAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static InjectorAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, InjectorAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<InjectorAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            InjectorAttributeMetadata.AttributeClassName
        );
        
        var generatedClassName = attributeData.GetNamedArgument<string>(nameof(InjectorAttribute.GeneratedClassName))
                                 ?? attributeData.GetConstructorArgument<string>(argument => argument.Kind != TypedConstantKind.Array);
            
        var specifications = attributeData
            .GetConstructorArguments<ITypeSymbol>(argument => argument.Kind != TypedConstantKind.Array)
            .Select(it => it.ToTypeModel())
            .ToEquatableList();
        
        return new InjectorAttributeMetadata(
            generatedClassName,
            specifications,
            attributeMetadata).ToOkResult();
    }
}
