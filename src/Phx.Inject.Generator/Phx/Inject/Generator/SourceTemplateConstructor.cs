// -----------------------------------------------------------------------------
// <copyright file="SourceTemplateConstructor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Common;
using Phx.Inject.Generator.Definitions;
using Phx.Inject.Generator.Model;
using Phx.Inject.Generator.Templates;

namespace Phx.Inject.Generator;

internal class SourceTemplateConstructor {
    public IReadOnlyList<(TypeModel, IRenderTemplate)> ConstructTemplates(
        IReadOnlyList<InjectionContextDefinition> injectionContextDefinitions,
        GeneratorExecutionContext context
    ) {
        try {
            var injectorDefinitions = injectionContextDefinitions
                .Select(injectionContextDefinition => injectionContextDefinition.Injector)
                .ToImmutableList();
            var injectorDefinitionMap = CreateTypeMap(
                injectorDefinitions,
                injector => injector.InjectorInterfaceType);

            return injectionContextDefinitions.SelectMany(injectionContextDefinition => {
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
        } catch (Exception e) {
            var diagnosticData = (e is InjectionException ie)
                ? ie.DiagnosticData
                : Diagnostics.UnexpectedError;
            
            throw new InjectionException(
                diagnosticData,
                $"An error occurred while constructing source templates.",
                Location.None,
                e);
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
