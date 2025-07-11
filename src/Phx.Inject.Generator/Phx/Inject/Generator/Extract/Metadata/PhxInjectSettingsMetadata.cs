﻿// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettingsExtractor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
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
            IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        private readonly PhxInjectAttributeMetadata.IExtractor phxInjectAttributeExtractor;

        public Extractor(PhxInjectAttributeMetadata.IExtractor phxInjectAttributeExtractor) {
            this.phxInjectAttributeExtractor = phxInjectAttributeExtractor;
        }

        public Extractor() : this(PhxInjectAttributeMetadata.Extractor.Instance) { }

        public GeneratorSettings Extract(
            IReadOnlyList<ITypeSymbol> settingsCandidates,
            IGeneratorContext generatorCtx
        ) {
            var extractorCtx = new ExtractorContext("extracting PhxInject settings", null, generatorCtx);
            IReadOnlyList<GeneratorSettings> injectSettings = settingsCandidates
                .Where(typeSymbol => phxInjectAttributeExtractor.CanExtract(typeSymbol))
                .SelectCatching(
                    generatorCtx.Aggregator,
                    typeSymbol => $"extracting PhxInject settings from {typeSymbol}",
                    typeSymbol => {
                        var settingsAttribute = phxInjectAttributeExtractor.Extract(typeSymbol, generatorCtx);

                        return new GeneratorSettings(
                            settingsAttribute.TabSize,
                            settingsAttribute.GeneratedFileExtension,
                            settingsAttribute.NullableEnabled,
                            settingsAttribute.AllowConstructorFactories,
                            settingsAttribute);
                    })
                .ToImmutableList();
            extractorCtx.Log($"Discovered {injectSettings.Count} inject settings.");

            var settings = injectSettings.Count switch {
                1 => injectSettings.First(),
                _ => generatorCtx.Aggregator.AggregateMany<GeneratorSettings, GeneratorSettings>(
                            injectSettings,
                            settings => $"extracting PhxInject settings {settings.Name}",
                            settings => throw Diagnostics.InvalidSpecification.AsException(
                                $"Only one PhxInject settings can be specified. "
                                + $"Found {injectSettings.Count} on types [{string.Join(", ", injectSettings.Select(it => it.Metadata?.AttributeMetadata.AttributedSymbol))}].",
                                settings.Location,
                                extractorCtx))
                        .FirstOrDefault()
                    ?? new GeneratorSettings()
            };
            extractorCtx.Log($"Using settings: {settings}");
            return settings;
        }
    }
}
