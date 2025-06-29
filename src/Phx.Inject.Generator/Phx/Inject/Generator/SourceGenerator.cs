// -----------------------------------------------------------------------------
//  <copyright file="SourceGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Definitions;
    using Phx.Inject.Generator.Descriptors;
    using Phx.Inject.Generator.Render;
    using Phx.Inject.Generator.Templates;

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
            Diagnostics.GeneratorExecutionContext = context;
            try {
                // Receive: Source code to syntax declarations.
                var syntaxReceiver = context.SyntaxReceiver as SourceSyntaxReceiver
                    ?? throw new InjectionException(
                        Diagnostics.UnexpectedError,
                        $"Incorrect Syntax Receiver {context.SyntaxReceiver}.",
                        Location.None);

                // Extract: Syntax declarations to descriptors.
                var sourceDescriptor = new SourceExtractor().Extract(syntaxReceiver, context);
                
                // Map: Descriptors to definitions.
                var injectionContextDefinitions = new SourceDefinitionMapper(generatorSettings)
                    .MapInjectionContexts(sourceDescriptor, context);

                // Construct: Definitions to templates.
                var templates = new SourceTemplateConstructor().ConstructTemplates(
                    injectionContextDefinitions,
                    context);

                // Render: Templates to generated source.
                new SourceRenderer(generatorSettings).RenderAllTemplates(templates, context);
            } catch (InjectionException ex) {
                foreach (var diagnostic in ex.ToDiagnostics()) {
                    context.ReportDiagnostic(diagnostic);
                }
            } catch (Exception ex) {
                context.ReportDiagnostic(Diagnostics.CreateUnexpectedErrorDiagnostic(ex.ToString()));
                Logger.Error("An unexpected error occurred while generating source.", ex);
            }
        }
    }
}
