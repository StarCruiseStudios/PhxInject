// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderPipeline.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Auto;

/// <summary>
///     Pipeline for processing AutoBuilder attributes into metadata.
/// </summary>
internal sealed class AutoBuilderPipeline(
    ICodeElementValidator elementValidator,
    ITransformer<IMethodSymbol, AutoBuilderMetadata> autoBuilderTransformer
) : ISyntaxValuesPipeline<AutoBuilderMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly AutoBuilderPipeline Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isAbstract: false
        ),
        AutoBuilderTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<AutoBuilderMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            AutoBuilderAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return autoBuilderTransformer
                    .Transform((IMethodSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }
}
