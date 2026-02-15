// -----------------------------------------------------------------------------
// <copyright file="SpecInterfacePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

/// <summary>
///     Pipeline for processing Specification interface declarations into metadata.
/// </summary>
internal sealed class SpecInterfacePipeline(
    ICodeElementValidator elementValidator,
    ITransformer<ITypeSymbol, SpecInterfaceMetadata> specInterfaceTransformer
) : ISyntaxValuesPipeline<SpecInterfaceMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly SpecInterfacePipeline Instance = new(
        SpecInterfaceMetadata.ElementValidator,
        SpecInterfaceTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<SpecInterfaceMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            SpecificationAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return specInterfaceTransformer
                    .Transform((ITypeSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }
}
