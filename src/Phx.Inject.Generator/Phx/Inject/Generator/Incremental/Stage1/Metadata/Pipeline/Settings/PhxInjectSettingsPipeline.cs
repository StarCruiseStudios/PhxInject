// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeSyntaxValuesProvider.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Settings;

internal class PhxInjectSettingsPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<PhxInjectAttributeMetadata> phxInjectAttributeTransformer
) : ISyntaxValuePipeline<PhxInjectSettingsMetadata> {
    public static readonly PhxInjectSettingsPipeline Instance = new(
        NoopCodeElementValidator.Instance,
        PhxInjectAttributeTransformer.Instance);

    public IncrementalValueProvider<PhxInjectSettingsMetadata> Select(SyntaxValueProvider syntaxProvider) {
        return syntaxProvider.ForAttributeWithMetadataName<PhxInjectAttributeMetadata>(
            PhxInjectAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => phxInjectAttributeTransformer.Transform(context.TargetSymbol))
            .Select((attributeMetadata, _) => new PhxInjectSettingsMetadata(attributeMetadata))
            .Collect()
            .Select((settings, cancellationToken) => settings.Length switch {
                0 => new PhxInjectSettingsMetadata(null),
                1 => settings[0],
                _ => throw new InvalidOperationException("Only one PhxInject attribute is allowed per assembly.")
            });
    }
}