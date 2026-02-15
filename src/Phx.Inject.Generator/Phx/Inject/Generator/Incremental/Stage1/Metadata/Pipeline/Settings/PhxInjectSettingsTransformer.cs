// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettingsTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections.Immutable;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Settings;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Settings;

/// <summary>
/// Transforms collected PhxInject attribute metadata into settings metadata.
/// </summary>
internal sealed class PhxInjectSettingsTransformer
    : ITransformer<ImmutableArray<IResult<PhxInjectAttributeMetadata>>, PhxInjectSettingsMetadata> {
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly PhxInjectSettingsTransformer Instance = new();

    /// <inheritdoc />
    public bool CanTransform(ImmutableArray<IResult<PhxInjectAttributeMetadata>> input) {
        return true;
    }

    /// <inheritdoc />
    public IResult<PhxInjectSettingsMetadata> Transform(
        ImmutableArray<IResult<PhxInjectAttributeMetadata>> attributes
    ) {
        return DiagnosticsRecorder.Capture<PhxInjectSettingsMetadata>(diagnostics => {
            return attributes.Length switch {
                0 => new PhxInjectSettingsMetadata(null),
                1 => new PhxInjectSettingsMetadata(attributes[0].OrThrow(diagnostics)),
                _ => new PhxInjectSettingsMetadata(attributes.Also(s => {
                    foreach (var result in s) {
                        if (result.TryGetValue(diagnostics, out var setting)) {
                            diagnostics.Add(new DiagnosticInfo(
                                DiagnosticType.UnexpectedError,
                                "Only one PhxInject settings attribute can be defined per assembly.",
                                setting.Location.Value)
                            );
                        }
                    }
                })[0].GetValue(diagnostics))
            };
        });
    }
}
