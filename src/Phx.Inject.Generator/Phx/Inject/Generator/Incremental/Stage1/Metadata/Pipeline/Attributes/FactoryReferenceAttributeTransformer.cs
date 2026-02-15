// -----------------------------------------------------------------------------
// <copyright file="FactoryReferenceAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Generator.Incremental.PhxInject;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms FactoryReference attribute data into metadata.
/// </summary>
/// <remarks>
///     Extracts the optional <c>FabricationMode</c> for parameters receiving factory delegates
///     instead of resolved instances. This enables lazy initialization, multiple instance creation,
///     and caller-controlled instantiation timing.
///
///     ## FabricationMode Values
///
///     - **Default (0 or unspecified)**: Uses the referenced factory's natural fabrication mode.
///     - **Recurrent**: Creates a new instance on every delegate invocation.
///     - **Scoped**: Returns the same instance on subsequent delegate calls within a scope.
///     - **Container/ContainerScoped**: Uses container-level caching for child injector scenarios.
/// </remarks>
internal sealed class FactoryReferenceAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<FactoryReferenceAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static FactoryReferenceAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    private const string FabricationModeClassName = $"{NamespaceName}.{nameof(FabricationMode)}";

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, FactoryReferenceAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<FactoryReferenceAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            FactoryReferenceAttributeMetadata.AttributeClassName
        );

        var fabricationMode =
            attributeData.GetNamedArgument<FabricationMode?>(nameof(FactoryReferenceAttribute.FabricationMode))
            ?? attributeData.GetConstructorArgument<FabricationMode>(argument =>
                argument.Type!.GetFullyQualifiedName() == FabricationModeClassName,
                default);

        return new FactoryReferenceAttributeMetadata(fabricationMode, attributeMetadata).ToOkResult();
    }
}
