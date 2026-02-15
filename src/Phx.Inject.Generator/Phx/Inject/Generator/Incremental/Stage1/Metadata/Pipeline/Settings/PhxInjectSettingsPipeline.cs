// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettingsPipeline.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Settings;

/// <summary>
/// Pipeline for processing PhxInject settings from assembly attributes.
/// </summary>
internal sealed class PhxInjectSettingsPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<PhxInjectAttributeMetadata> phxInjectAttributeTransformer
) : ISyntaxValuePipeline<PhxInjectSettingsMetadata> {
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly PhxInjectSettingsPipeline Instance = new(
        NoopCodeElementValidator.Instance,
        PhxInjectAttributeTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValueProvider<IResult<PhxInjectSettingsMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            PhxInjectAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => phxInjectAttributeTransformer.Transform(context.TargetSymbol))
            .Select((attributeMetadata, _) => DiagnosticsRecorder.Capture(diagnostics => {
                    return new PhxInjectSettingsMetadata(attributeMetadata.OrThrow(diagnostics));
            }))
            .Collect()
            .Select((settings, _) => DiagnosticsRecorder.Capture<PhxInjectSettingsMetadata>(diagnostics => {
                return settings.Length switch {
                    0 => new PhxInjectSettingsMetadata(null),
                    1 => settings[0].OrThrow(diagnostics),
                    _ => settings.Also(s => {
                        foreach (var result in s) {
                            if (result.TryGetValue(diagnostics, out var setting)) {
                                diagnostics.Add(new DiagnosticInfo(
                                    DiagnosticType.UnexpectedError,
                                    "Only one PhxInject settings attribute can be defined per assembly.",
                                    setting.Location.Value)
                                );
                            }
                        }
                    })[0].GetValue(diagnostics)
                };
            }));
    }
}
