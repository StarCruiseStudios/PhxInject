// -----------------------------------------------------------------------------
//  <copyright file="SourceGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

// #define ATTACH_DEBUGGER

namespace Phx.Inject.Generator {
#if ATTACH_DEBUGGER
    using System.Threading;
#endif
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Manager;
    using Phx.Inject.Generator.Model;
    using Phx.Inject.Generator.Model.Descriptors;
    using Phx.Inject.Generator.Model.Templates;
    using Phx.Inject.Generator.Presenter;
    using Phx.Inject.Generator.Render;

    [Generator]
    internal class SourceGenerator : ISourceGenerator {
        private const string GeneratedFileExtension = "generated.cs";

        public void Initialize(GeneratorInitializationContext context) {
#if ATTACH_DEBUGGER
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) {
                Thread.Sleep(500);
            }
#endif
            context.RegisterForSyntaxNotifications(() => new InjectorSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            var injectorExtractor = new InjectorExtractor();
            var specExtractor = new SpecExtractor();

            var injectionController = new InjectionController();

            var injectorPresenter = new InjectorPresenter();
            var specContainerPresenter = new SpecContainerPresenter();

            var renderSettings = new RenderSettings();
            var templateRenderer = new TemplateRenderer(() => new RenderWriter(renderSettings));

            try {
                var syntaxReceiver = context.SyntaxReceiver as InjectorSyntaxReceiver
                        ?? throw new InjectionException(
                                Diagnostics.InvalidSpecification,
                                $"Incorrect Syntax Receiver {context.SyntaxReceiver}.",
                                Location.None);

                // Extract: SyntaxDeclarations to Descriptors
                var specDescriptors = specExtractor.Extract(
                        syntaxReceiver.SpecificationCandidates,
                        context);
                Logger.Info($"Discovered {specDescriptors.Count} specifications.");

                var specDescriptorMap = new Dictionary<TypeModel, SpecDescriptor>();
                foreach (var specDescriptor in specDescriptors) {
                    if (specDescriptorMap.ContainsKey(specDescriptor.SpecType)) {
                        throw new InjectionException(
                                Diagnostics.InvalidSpecification,
                                $"Specification for type {specDescriptor.SpecType} is already defined.",
                                specDescriptor.Location);
                    }

                    specDescriptorMap.Add(specDescriptor.SpecType, specDescriptor);
                }

                var injectorDescriptors = injectorExtractor.Extract(
                        syntaxReceiver.InjectorCandidates,
                        context,
                        specDescriptorMap);
                Logger.Info($"Discovered {injectorDescriptors.Count} injectors.");

                foreach (var injectorDescriptor in injectorDescriptors) {
                    // Map: Descriptors to Definitions
                    var injectionContextDefinition = injectionController.Map(injectorDescriptor);

                    // Construct: Definitions to Templates
                    var injectorDefinition = injectionContextDefinition.Injector;
                    var templates = new List<(TypeModel, IRenderTemplate)>();
                    foreach (var specContainerDefinition in injectionContextDefinition.SpecContainers) {
                        templates.Add(
                                (specContainerDefinition.ContainerType,
                                        specContainerPresenter.Generate(specContainerDefinition)));
                        Logger.Info(
                                $"Generated spec container {specContainerDefinition.ContainerType} for injector {injectorDefinition.InjectorType}.");
                    }

                    templates.Add(
                            (injectorDefinition.InjectorType,
                                    injectorPresenter.Generate(injectorDefinition)));
                    Logger.Info($"Generated injector {injectorDefinition.InjectorType}.");

                    // Render: Templates to Source.
                    foreach (var (classType, template) in templates) {
                        var fileName = $"{classType.QualifiedName}.{GeneratedFileExtension}";
                        Logger.Info($"Rendering source for {fileName}");
                        templateRenderer.RenderTemplate(fileName, template, context);
                    }
                }
            } catch (InjectionException ex) {
                context.ReportDiagnostic(
                        Diagnostic.Create(
                                new DiagnosticDescriptor(
                                        id: ex.DiagnosticData.Id,
                                        title: ex.DiagnosticData.Title,
                                        messageFormat: ex.Message,
                                        category: ex.DiagnosticData.Category,
                                        defaultSeverity: DiagnosticSeverity.Error,
                                        isEnabledByDefault: true),
                                ex.Location));
            } catch (Exception ex) {
                context.ReportDiagnostic(
                        Diagnostic.Create(
                                new DiagnosticDescriptor(
                                        id: Diagnostics.UnexpectedError.Id,
                                        title: Diagnostics.UnexpectedError.Title,
                                        messageFormat: ex.ToString(),
                                        category: Diagnostics.UnexpectedError.Category,
                                        defaultSeverity: DiagnosticSeverity.Error,
                                        isEnabledByDefault: true),
                                Location.None));
                Logger.Error("An unexpected error occurred while generating source.", ex);
                throw;
            }
        }
    }
}
