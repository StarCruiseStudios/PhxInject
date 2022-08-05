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
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Manager;
    using Phx.Inject.Generator.Model;
    using Phx.Inject.Generator.Presenter;
    using Phx.Inject.Generator.Render;

    [Generator]
    internal class SourceGenerator : ISourceGenerator {
        private readonly RenderSettings renderSettings;

        public SourceGenerator() : this(new RenderSettings()) { }

        public SourceGenerator(RenderSettings renderSettings) {
            this.renderSettings = renderSettings;
        }

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
            try {
                var syntaxReceiver = context.SyntaxReceiver as InjectorSyntaxReceiver
                        ?? throw new InjectionException(
                                Diagnostics.InvalidSpecification,
                                $"Incorrect Syntax Receiver {context.SyntaxReceiver}.",
                                Location.None);

                //
                // Extract: SyntaxDeclarations to Descriptors
                //
                var descriptorGenerationContext = new DescriptorGenerationContext(context);

                var injectorDescriptors = new InjectorExtractor().Extract(
                        syntaxReceiver.InjectorCandidates,
                        descriptorGenerationContext);
                Logger.Info($"Discovered {injectorDescriptors.Count} injector types.");

                var specDescriptors = new SpecExtractor().Extract(
                        syntaxReceiver.SpecificationCandidates,
                        descriptorGenerationContext);
                Logger.Info($"Discovered {specDescriptors.Count} specification types.");

                var externalDependencyDescriptors = new ExternalDependencyExtractor().Extract(
                        syntaxReceiver.InjectorCandidates,
                        descriptorGenerationContext);
                Logger.Info($"Discovered {externalDependencyDescriptors.Count} external dependency types.");

                var allSpecDescriptors = externalDependencyDescriptors
                        .Select(dep => dep.GetSpecDescriptor())
                        .Concat(specDescriptors)
                        .ToImmutableList();

                //
                // Map: Descriptors to Definitions
                //
                var injectorDescriptorMap = CreateTypeMap(
                        injectorDescriptors,
                        injector => injector.InjectorInterfaceType);
                var specDescriptorMap = CreateTypeMap(
                        allSpecDescriptors,
                        spec => spec.SpecType);
                var externalDependencyDescriptorMap = CreateTypeMap(
                        externalDependencyDescriptors,
                        dep => dep.ExternalDependencyInterfaceType);

                var injectionContextDefinitions = injectorDescriptors.Select(
                                injectorDescriptor => {
                                    var definitionGenerationContext = new DefinitionGenerationContext(
                                            injectorDescriptor,
                                            injectorDescriptorMap,
                                            specDescriptorMap,
                                            externalDependencyDescriptorMap,
                                            ImmutableDictionary<RegistrationIdentifier, FactoryRegistration>.Empty,
                                            ImmutableDictionary<RegistrationIdentifier, BuilderRegistration>.Empty,
                                            context);

                                    return new InjectionController().Map(definitionGenerationContext);
                                })
                        .ToImmutableList();

                //
                // Construct: Definitions to Templates
                //
                var injectorDefinitions = injectionContextDefinitions.Select(
                        injectionContextDefinition => injectionContextDefinition.Injector)
                        .ToImmutableList();
                var injectorDefinitionMap = CreateTypeMap(
                        injectorDefinitions,
                        injector => injector.InjectorInterfaceType);

                var templates = injectionContextDefinitions.SelectMany(
                        injectionContextDefinition => {
                            var specDefinitionMap = CreateTypeMap(
                                    injectionContextDefinition.SpecContainers,
                                    spec => spec.SpecificationType);
                            var externalDependencyDefinitionMap = CreateTypeMap(
                                    injectionContextDefinition.ExternalDependencyImplementations,
                                    dep => dep.ExternalDependencyInterfaceType);

                            var templateGenerationContext = new TemplateGenerationContext(
                                    injectionContextDefinition.Injector,
                                    injectorDefinitionMap,
                                    specDefinitionMap,
                                    externalDependencyDefinitionMap,
                                    context);

                            var templates = new List<(TypeModel, IRenderTemplate)>();
                            var injectorDefinition = injectionContextDefinition.Injector;
                            templates.Add((
                                    injectorDefinition.InjectorType,
                                    new InjectorPresenter().Generate(injectorDefinition, templateGenerationContext)
                            ));
                            Logger.Info($"Generated injector {injectorDefinition.InjectorType}.");

                            var specContainerPresenter = new SpecContainerPresenter();
                            foreach (var specContainerDefinition in injectionContextDefinition.SpecContainers) {
                                templates.Add((
                                        specContainerDefinition.SpecContainerType,
                                        specContainerPresenter.Generate(specContainerDefinition, templateGenerationContext)
                                ));
                                Logger.Info(
                                        $"Generated spec container {specContainerDefinition.SpecContainerType} for injector {injectorDefinition.InjectorType}.");
                            }

                            var externalDependencyImplementationPresenter = new ExternalDependencyImplementationPresenter();
                            foreach (var dependency in injectionContextDefinition.ExternalDependencyImplementations) {
                                templates.Add((
                                        dependency.ExternalDependencyImplementationType,
                                        externalDependencyImplementationPresenter.Generate(dependency, templateGenerationContext)
                                ));
                                Logger.Info(
                                        $"Generated external dependency implementation {dependency.ExternalDependencyImplementationType} for injector {injectorDefinition.InjectorType}.");
                            }

                            return templates;
                        }).ToImmutableList();

                //
                // Render: Templates to Source.
                //
                var templateRenderer = new TemplateRenderer(() => new RenderWriter(renderSettings));
                foreach (var (classType, template) in templates) {
                    var fileName = $"{classType.QualifiedName}.{renderSettings.GeneratedFileExtension}";
                    Logger.Info($"Rendering source for {fileName}");
                    templateRenderer.RenderTemplate(fileName, template, context);
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

        private static IReadOnlyDictionary<TypeModel, T> CreateTypeMap<T>(
                IEnumerable<T> values,
                Func<T, TypeModel> extractKey
        ) where T : ISourceCodeElement  {
            var map = new Dictionary<TypeModel, T>();
            foreach (var value in values) {
                var key = extractKey(value);
                if (map.ContainsKey(key)) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"{typeof(T).Name} with {key} is already defined.",
                            value.Location);
                }

                map.Add(key, value);
            }

            return map;
        }
    }
}
