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
        IReadOnlyList<InjectionContextDef> injectionContextDefs,
        GeneratorExecutionContext context
    ) {
        try {
            IReadOnlyList<InjectorDef> injectorDefs = injectionContextDefs
                .Select(injectionContextDef => injectionContextDef.Injector)
                .ToImmutableList();
            var injectorDefMap = CreateTypeMap(
                injectorDefs,
                injector => injector.InjectorInterfaceType);

            return injectionContextDefs.SelectMany(injectionContextDef => {
                    var specDefMap = CreateTypeMap(
                        injectionContextDef.SpecContainers,
                        spec => spec.SpecificationType);
                    var dependencyDefMap = CreateTypeMap(
                        injectionContextDef.DependencyImplementations,
                        dep => dep.DependencyInterfaceType);

                    var templateGenerationContext = new TemplateGenerationContext(
                        injectionContextDef.Injector,
                        injectorDefMap,
                        specDefMap,
                        dependencyDefMap,
                        context);

                    var templates = new List<(TypeModel, IRenderTemplate)>();
                    var injectorDef = injectionContextDef.Injector;
                    templates.Add(
                        (
                            injectorDef.InjectorType,
                            new InjectorConstructor().Construct(
                                injectorDef,
                                templateGenerationContext)
                        ));
                    Logger.Info($"Generated injector {injectorDef.InjectorType}.");

                    var specContainerPresenter = new SpecContainerConstructor();
                    foreach (var specContainerDef in injectionContextDef.SpecContainers) {
                        templates.Add(
                            (
                                specContainerDef.SpecContainerType,
                                specContainerPresenter.Construct(
                                    specContainerDef,
                                    templateGenerationContext)
                            ));
                        Logger.Info(
                            $"Generated spec container {specContainerDef.SpecContainerType} for injector {injectorDef.InjectorType}.");
                    }

                    var dependencyImplementationPresenter
                        = new DependencyImplementationConstructor();
                    foreach (var dependency in injectionContextDef
                        .DependencyImplementations) {
                        templates.Add(
                            (
                                dependency.DependencyImplementationType,
                                dependencyImplementationPresenter.Construct(
                                    dependency,
                                    templateGenerationContext)
                            ));
                        Logger.Info(
                            $"Generated dependency implementation {dependency.DependencyImplementationType} for injector {injectorDef.InjectorType}.");
                    }

                    return templates;
                })
                .ToImmutableList();
        } catch (Exception e) {
            var diagnosticData = e is InjectionException ie
                ? ie.DiagnosticData
                : Diagnostics.UnexpectedError;

            throw new InjectionException(
                diagnosticData,
                "An error occurred while constructing source templates.",
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
