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
using Phx.Inject.Generator.Abstract;
using Phx.Inject.Generator.Extract;
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

    public void Execute(GeneratorExecutionContext context) {
        try {
            // Abstract: Source code to syntax declarations.
            var syntaxReceiver = context.SyntaxReceiver as SourceSyntaxReceiver
                ?? throw new InjectionException(
                    context,
                    Diagnostics.UnexpectedError,
                    $"Incorrect Syntax Receiver {context.SyntaxReceiver}.",
                    Location.None);

            var settingsCandidates = 
                MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxReceiver.PhxInjectSettingsCandidates, context)
                .Where(symbol => TypeHelpers.IsPhxInjectSettingsSymbol(symbol, context))
                .ToImmutableList();
                
            var settings = settingsCandidates.Count switch {
                0 => generatorSettings,
                1 => MetadataHelpers.GetGeneratorSettings(settingsCandidates.First().GetPhxInjectAttribute(context)!, context),
                _ => throw new InjectionException(
                    context,
                    Diagnostics.UnexpectedError,
                    $"Only one PhxInject settings can be specified. Found {settingsCandidates.Count} on types [{string.Join(", ", settingsCandidates.Select(it => it.Name))}].",
                    Location.None)
            };

            // Extract: Syntax declarations to descriptors.
            var sourceDesc = new SourceExtractor().Extract(syntaxReceiver, context);

            // Map: Descriptors to defs.
            var injectionContextDefs = new SourceDefMapper(settings)
                .Map(sourceDesc, context);

            // Project: Defs to templates.
            var templates = new SourceTemplateProjector().Project(
                injectionContextDefs,
                context);

            // Render: Templates to generated source.
            new SourceRenderer(settings).Render(templates, context);
        } catch (Exception ex) {
            if (ex is not InjectionException) {
                context.ReportDiagnostic(Diagnostics.UnexpectedError.CreateDiagnostic(ex.ToString()));
            }
        }
    }
}
