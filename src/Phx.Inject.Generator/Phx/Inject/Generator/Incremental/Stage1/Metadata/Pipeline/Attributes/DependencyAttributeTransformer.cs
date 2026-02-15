// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Dependency attribute data into metadata.
/// </summary>
/// <remarks>
///     Extracts dependency interface type from <c>[Dependency]</c> attributes marking external
///     dependencies required by the injector. Unlike most transformers, validates immediately that
///     dependency type is a public/internal interface, providing early feedback before downstream
///     transformers attempt code generation with invalid types.
/// </remarks>
internal sealed class DependencyAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer,
    ICodeElementValidator dependencyTypeValidator
) : IAttributeTransformer<DependencyAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static DependencyAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance,
        DependencyAttributeMetadata.ElementValidator
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, DependencyAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<DependencyAttributeMetadata> Transform(ISymbol targetSymbol) {
            var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
                targetSymbol,
                DependencyAttributeMetadata.AttributeClassName
            );

            var dependencyTypeSymbol = attributeData
                .GetConstructorArgument<ITypeSymbol>(argument => argument.Kind != TypedConstantKind.Array);

            if (!dependencyTypeValidator.IsValidSymbol(dependencyTypeSymbol)) {
                return Result.Error<DependencyAttributeMetadata>(new DiagnosticInfo(
                    DiagnosticType.UnexpectedError,
                    "The specified dependency type is invalid.",
                    LocationInfo.CreateFrom(targetSymbol.GetLocationOrDefault())
                ));
            }
            
            return new DependencyAttributeMetadata(dependencyTypeSymbol.ToTypeModel(), attributeMetadata).ToOkResult();
    }
}
