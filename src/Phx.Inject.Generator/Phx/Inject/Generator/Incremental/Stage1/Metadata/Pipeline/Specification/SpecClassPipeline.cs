// -----------------------------------------------------------------------------
// <copyright file="SpecClassPipeline.cs" company="Star Cruise Studios LLC">
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
///     Pipeline for processing Specification class declarations into metadata.
/// </summary>
internal sealed class SpecClassPipeline(
    ICodeElementValidator elementValidator,
    ITransformer<ITypeSymbol, SpecClassMetadata> specClassTransformer
) : ISyntaxValuesPipeline<SpecClassMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly SpecClassPipeline Instance = new(
        ClassElementValidator.PublicStaticClass,
        SpecClassTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<SpecClassMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            SpecificationAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return specClassTransformer
                    .Transform((ITypeSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }
}
