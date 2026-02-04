// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettingsMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal static class PhxInjectSettingsMetadata {
    public interface IExtractor {
        GeneratorSettings Extract(
            IReadOnlyList<ITypeSymbol> settingsCandidates,
            IGeneratorContext parentCtx);
    }

    public class Extractor(PhxInjectAttributeMetadata.IExtractor phxInjectAttributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            PhxInjectAttributeMetadata.Extractor.Instance
        );

        public GeneratorSettings Extract(
            IReadOnlyList<ITypeSymbol> settingsCandidates,
            IGeneratorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting PhxInject settings for assembly {parentCtx.ExecutionContext.Compilation.Assembly}",
                parentCtx.ExecutionContext.Compilation.Assembly,
                currentCtx => {
                    IReadOnlyList<GeneratorSettings> injectSettings = settingsCandidates
                        .Where(phxInjectAttributeExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            typeSymbol => $"extracting PhxInject settings from {typeSymbol}",
                            typeSymbol => {
                                var settingsAttribute = phxInjectAttributeExtractor.Extract(typeSymbol, currentCtx);

                                return new GeneratorSettings(
                                    settingsAttribute.TabSize,
                                    settingsAttribute.GeneratedFileExtension,
                                    settingsAttribute.NullableEnabled,
                                    settingsAttribute.AllowConstructorFactories,
                                    settingsAttribute);
                            })
                        .ToImmutableList();
                    currentCtx.Log($"Discovered {injectSettings.Count} inject settings.");

                    var settings = injectSettings.Count switch {
                        1 => injectSettings.First(),
                        _ => currentCtx.Aggregator.AggregateMany<GeneratorSettings, GeneratorSettings>(
                                    injectSettings,
                                    settings => $"extracting PhxInject settings {settings.Name}",
                                    settings => throw Diagnostics.InvalidSpecification.AsException(
                                        $"Only one PhxInject settings can be specified. "
                                        + $"Found {injectSettings.Count} on types [{string.Join(", ", injectSettings.Select(it => it.Metadata?.AttributeMetadata.AttributedSymbol))}].",
                                        settings.Location,
                                        currentCtx))
                                .FirstOrDefault()
                            ?? new GeneratorSettings()
                    };
                    currentCtx.Log($"Using settings: {settings}");
                    return settings;
                });
        }
    }
}
