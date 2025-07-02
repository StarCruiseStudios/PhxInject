// -----------------------------------------------------------------------------
//  <copyright file="SourceGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

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
                    Diagnostics.UnexpectedError,
                    $"Incorrect Syntax Receiver {context.SyntaxReceiver}.",
                    Location.None);

            // Extract: Syntax declarations to descriptors.
            var sourceDesc = new SourceExtractor().Extract(syntaxReceiver, context);

            // Map: Descriptors to defs.
            var injectionContextDefs = new SourceDefMapper(generatorSettings)
                .Map(sourceDesc, context);

            // Project: Defs to templates.
            var templates = new SourceTemplateProjector().Project(
                injectionContextDefs,
                context);

            // Render: Templates to generated source.
            new SourceRenderer(generatorSettings).RenderAllTemplates(templates, context);
        } catch (InjectionException ex) {
            foreach (var diagnostic in ex.Diagnostics) {
                context.ReportDiagnostic(diagnostic);
            }
        } catch (Exception ex) {
            context.ReportDiagnostic(Diagnostics.UnexpectedError.CreateDiagnostic(ex.ToString()));
        }
    }
}
