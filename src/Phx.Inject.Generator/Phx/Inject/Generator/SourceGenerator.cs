// -----------------------------------------------------------------------------
//  <copyright file="SourceGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;
    using Phx.Inject.Generator.Common.Descriptors;
    using Phx.Inject.Generator.Common.Render;
    using Phx.Inject.Generator.Common.Templates;
    using Phx.Inject.Generator.External.Descriptors;
    using Phx.Inject.Generator.External.Templates;
    using Phx.Inject.Generator.Injectors.Descriptors;
    using Phx.Inject.Generator.Injectors.Templates;
    using Phx.Inject.Generator.Specifications.Descriptors;
    using Phx.Inject.Generator.Specifications.Templates;

    [Generator]
    internal class SourceGenerator : ISourceGenerator {
        private readonly GeneratorSettings generatorSettings;

        public SourceGenerator() : this(new GeneratorSettings()) { }

        public SourceGenerator(GeneratorSettings generatorSettings) {
            this.generatorSettings = generatorSettings;
        }

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new InjectorSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            Diagnostics.GeneratorExecutionContext = context;
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
                                    var injectorSpecDescriptorMap = new Dictionary<TypeModel, SpecDescriptor>();
                                    foreach (var spec in injectorDescriptor.SpecificationsTypes) {
                                        if (!specDescriptorMap.TryGetValue(spec, out var specDescriptor)) {
                                            throw new InjectionException(
                                                    Diagnostics.IncompleteSpecification,
                                                    $"Cannot find required specification type {spec}"
                                                    + $" while generating injection for type {injectorDescriptor.InjectorInterfaceType}.",
                                                    injectorDescriptor.Location);
                                        }

                                        injectorSpecDescriptorMap.Add(spec, specDescriptor);
                                    }
                                    if (generatorSettings.AllowConstructorFactories) {
                                        var constructorSpecs = new SpecExtractor()
                                                .ExtractConstructorSpecForContext(new DefinitionGenerationContext(
                                                        injectorDescriptor,
                                                        injectorDescriptorMap,
                                                        injectorSpecDescriptorMap,
                                                        externalDependencyDescriptorMap,
                                                        ImmutableDictionary<RegistrationIdentifier, List<FactoryRegistration>>.Empty,
                                                        ImmutableDictionary<RegistrationIdentifier, BuilderRegistration>.Empty,
                                                        context));
                                        
                                        foreach (var constructorSpec in constructorSpecs) {
                                            injectorSpecDescriptorMap.Add(constructorSpec.SpecType, constructorSpec);
                                        }
                                    }

                                    var definitionGenerationContext = new DefinitionGenerationContext(
                                            injectorDescriptor,
                                            injectorDescriptorMap,
                                            injectorSpecDescriptorMap,
                                            externalDependencyDescriptorMap,
                                            ImmutableDictionary<RegistrationIdentifier, List<FactoryRegistration>>.Empty,
                                            ImmutableDictionary<RegistrationIdentifier, BuilderRegistration>.Empty,
                                            context);

                                    return new InjectionDefinitionMapper().Map(definitionGenerationContext);
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
                                    templates.Add(
                                            (
                                                    injectorDefinition.InjectorType,
                                                    new InjectorConstructor().Construct(
                                                            injectorDefinition,
                                                            templateGenerationContext)
                                            ));
                                    Logger.Info($"Generated injector {injectorDefinition.InjectorType}.");

                                    var specContainerPresenter = new SpecContainerConstructor();
                                    foreach (var specContainerDefinition in injectionContextDefinition.SpecContainers) {
                                        templates.Add(
                                                (
                                                        specContainerDefinition.SpecContainerType,
                                                        specContainerPresenter.Construct(
                                                                specContainerDefinition,
                                                                templateGenerationContext)
                                                ));
                                        Logger.Info(
                                                $"Generated spec container {specContainerDefinition.SpecContainerType} for injector {injectorDefinition.InjectorType}.");
                                    }

                                    var externalDependencyImplementationPresenter
                                            = new ExternalDependencyImplementationConstructor();
                                    foreach (var dependency in injectionContextDefinition
                                            .ExternalDependencyImplementations) {
                                        templates.Add(
                                                (
                                                        dependency.ExternalDependencyImplementationType,
                                                        externalDependencyImplementationPresenter.Construct(
                                                                dependency,
                                                                templateGenerationContext)
                                                ));
                                        Logger.Info(
                                                $"Generated external dependency implementation {dependency.ExternalDependencyImplementationType} for injector {injectorDefinition.InjectorType}.");
                                    }

                                    return templates;
                                })
                        .ToImmutableList();

                //
                // Render: Templates to Source.
                //
                var templateRenderer = new TemplateRenderer(() => new RenderWriter(generatorSettings));
                foreach (var (classType, template) in templates) {
                    var fileName = $"{classType.QualifiedName}.{generatorSettings.GeneratedFileExtension}";
                    Logger.Info($"Rendering source for {fileName}");
                    templateRenderer.RenderTemplate(fileName, template, context);
                }
            } catch (InjectionException ex) {
                context.ReportDiagnostic(
                        Diagnostic.Create(
                                new DiagnosticDescriptor(
                                        ex.DiagnosticData.Id,
                                        ex.DiagnosticData.Title,
                                        ex.Message,
                                        ex.DiagnosticData.Category,
                                        DiagnosticSeverity.Error,
                                        isEnabledByDefault: true),
                                ex.Location));
            } catch (Exception ex) {
                context.ReportDiagnostic(
                        Diagnostic.Create(
                                new DiagnosticDescriptor(
                                        Diagnostics.UnexpectedError.Id,
                                        Diagnostics.UnexpectedError.Title,
                                        ex.ToString(),
                                        Diagnostics.UnexpectedError.Category,
                                        DiagnosticSeverity.Error,
                                        isEnabledByDefault: true),
                                Location.None));
                Logger.Error("An unexpected error occurred while generating source.", ex);
                throw;
            }
        }

        private static IReadOnlyDictionary<TypeModel, T> CreateTypeMap<T>(
                IEnumerable<T> values,
                Func<T, TypeModel> extractKey
        ) where T : ISourceCodeElement {
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
