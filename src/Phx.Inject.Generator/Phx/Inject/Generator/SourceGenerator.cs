// -----------------------------------------------------------------------------
//  <copyright file="SourceGenerator.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Map;
using Phx.Inject.Generator.Project;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Generator;

[Generator]
internal class SourceGenerator : ISourceGenerator {
    private readonly GeneratorSettings generatorSettings;

    public SourceGenerator() : this(new GeneratorSettings()) { }

    public SourceGenerator(GeneratorSettings generatorSettings) {
        this.generatorSettings = generatorSettings;
    }

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new SourceSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext executionCtx) {
        var generatorCtx = new GeneratorContext(executionCtx);
        try {
            ExceptionAggregator.Try(
                "generating injection types",
                generatorCtx,
                exceptionAggregator => {
                    // Abstract: Source code to syntax declarations.
                    var syntaxReceiver = generatorCtx.ExecutionContext.SyntaxReceiver as SourceSyntaxReceiver
                        ?? throw Diagnostics.UnexpectedError.AsException(
                            $"Incorrect Syntax Receiver {generatorCtx.ExecutionContext.SyntaxReceiver}.",
                            Location.None,
                            generatorCtx);

                    IReadOnlyList<ITypeSymbol> settingsSymbols = syntaxReceiver.PhxInjectSettingsCandidates
                        .SelectCatching(
                            exceptionAggregator,
                            syntaxNode => $"reading PhxInject settings from {syntaxNode.Identifier.Text}",
                            syntaxNode => {
                                var settingsSymbol = MetadataHelpers
                                    .ExpectTypeSymbolFromDeclaration(syntaxNode, generatorCtx)
                                    .GetOrThrow(generatorCtx);
                                return TypeHelpers.IsPhxInjectSettingsSymbol(settingsSymbol)
                                    .GetOrThrow(generatorCtx)
                                    ? settingsSymbol
                                    : null;
                            })
                        .OfType<ITypeSymbol>()
                        .ToImmutableList();

                    var settings = settingsSymbols.Count switch {
                        0 => generatorSettings,
                        1 => MetadataHelpers.GetGeneratorSettings(settingsSymbols.First()
                            .ExpectPhxInjectAttribute()
                            .GetOrThrow(generatorCtx)),
                        _ => Result.Error<GeneratorSettings>(
                                $"Only one PhxInject settings can be specified. Found {settingsSymbols.Count} on types [{string.Join(", ", settingsSymbols.Select(it => it.Name))}].",
                                Location.None,
                                Diagnostics.UnexpectedError)
                            .GetOrThrow(generatorCtx)
                    };

                    // Extract: Syntax declarations to descriptors.
                    var sourceDesc = new SourceDesc.Extractor()
                        .Extract(syntaxReceiver, generatorCtx);

                    // Map: Descriptors to defs.
                    var injectionContextDefs = new SourceDefMapper(settings)
                        .Map(sourceDesc, generatorCtx);

                    // Project: Defs to templates.
                    var templates = new SourceTemplateProjector()
                        .Project(injectionContextDefs, generatorCtx);

                    // Render: Templates to generated source.
                    new SourceRenderer(settings)
                        .Render(templates, generatorCtx);
                });
        } catch (InjectionException) {
            // Ignore injection exceptions to allow partial generation to complete.
            // They are already reported as diagnostics.
        }
    }
}
