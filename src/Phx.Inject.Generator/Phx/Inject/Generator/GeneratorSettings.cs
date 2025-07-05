// -----------------------------------------------------------------------------
//  <copyright file="GeneratorSettings.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------


using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Abstract;
using Phx.Inject.Generator.Extract;

namespace Phx.Inject.Generator;

internal record GeneratorSettings(
    string Name,
    Location Location,
    int TabSize = 4,
    string GeneratedFileExtension = "generated.cs",
    bool NullableEnabled = true,
    bool AllowConstructorFactories = true
) {
    public interface IExtractor {
        GeneratorSettings Extract(SourceSyntaxReceiver syntaxReceiver, IExceptionAggregator exceptionAggregator, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public GeneratorSettings Extract(
            SourceSyntaxReceiver syntaxReceiver,
            IExceptionAggregator exceptionAggregator,
            IGeneratorContext generatorCtx
        ) {
            var extractorCtx = new ExtractorContext(generatorCtx.ExecutionContext);
            IReadOnlyList<GeneratorSettings> injectSettings = syntaxReceiver.PhxInjectSettingsCandidates
                .SelectCatching(
                    exceptionAggregator,
                    syntaxNode => $"extracting PhxInject settings from {syntaxNode.Identifier.Text}",
                    syntaxNode => {
                        var settingsSymbol = MetadataHelpers
                            .ExpectTypeSymbolFromDeclaration(syntaxNode, extractorCtx)
                            .GetOrThrow(extractorCtx);

                        var settingsAttribute =
                            settingsSymbol.TryGetPhxInjectAttribute().GetOrThrow(extractorCtx);
                        if (settingsAttribute == null) {
                            return null;
                        }

                        if (settingsSymbol is not { TypeKind: TypeKind.Class }) {
                            throw Diagnostics.InvalidSpecification.AsException(
                                $"PhxInject settings type {settingsSymbol.Name} must be a class.",
                                settingsSymbol.Locations.First(),
                                generatorCtx);
                        }
                        
                        return MetadataHelpers.GetGeneratorSettings(settingsSymbol, settingsAttribute);
                    })
                .OfType<GeneratorSettings>()
                .ToImmutableList();
            extractorCtx.Log($"Discovered {injectSettings.Count} inject settings.");

            var settings = injectSettings.Count switch {
                1 => injectSettings.First(),
                _ => injectSettings.SelectCatching<GeneratorSettings, GeneratorSettings>(
                    exceptionAggregator,
                    settings => $"extracting PhxInject settings {settings.Name}",
                    settings => throw Diagnostics.InvalidSpecification.AsException(
                        $"Only one PhxInject settings can be specified. Found {injectSettings.Count} on types [{string.Join(", ", injectSettings.Select(it => it.Name))}].",
                        settings.Location,
                        generatorCtx)).FirstOrDefault() ?? new GeneratorSettings("Default", Location.None)
            };

            extractorCtx.Log($"Using settings: {settings}");
            return settings;
        }
    }
}
