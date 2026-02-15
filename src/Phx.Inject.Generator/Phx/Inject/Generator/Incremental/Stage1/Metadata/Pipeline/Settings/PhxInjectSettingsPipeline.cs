// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettingsPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Settings;

/// <summary>
/// Pipeline for processing PhxInject settings from assembly attributes.
/// </summary>
internal sealed class PhxInjectSettingsPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<PhxInjectAttributeMetadata> phxInjectAttributeTransformer,
    ITransformer<ImmutableArray<IResult<PhxInjectAttributeMetadata>>, PhxInjectSettingsMetadata> settingsTransformer
) : ISyntaxValuePipeline<PhxInjectSettingsMetadata> {
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly PhxInjectSettingsPipeline Instance = new(
        NoopCodeElementValidator.Instance,
        PhxInjectAttributeTransformer.Instance,
        PhxInjectSettingsTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValueProvider<IResult<PhxInjectSettingsMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            PhxInjectAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => phxInjectAttributeTransformer.Transform(context.TargetSymbol))
            .Collect()
            .Select((attributes, _) => settingsTransformer.Transform(attributes));
    }
}
