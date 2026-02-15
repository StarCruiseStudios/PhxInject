// -----------------------------------------------------------------------------
// <copyright file="InjectorInterfacePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;

/// <summary>
///     Pipeline for processing Injector interface declarations into metadata.
/// </summary>
internal sealed class InjectorInterfacePipeline(
    ICodeElementValidator elementValidator,
    ITransformer<ITypeSymbol, InjectorInterfaceMetadata> injectorInterfaceTransformer
) : ISyntaxValuesPipeline<InjectorInterfaceMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly InjectorInterfacePipeline Instance = new(
        new InterfaceElementValidator(
            CodeElementAccessibility.PublicOrInternal
        ),
        InjectorInterfaceTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<InjectorInterfaceMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            InjectorAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return injectorInterfaceTransformer
                    .Transform((ITypeSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }
}