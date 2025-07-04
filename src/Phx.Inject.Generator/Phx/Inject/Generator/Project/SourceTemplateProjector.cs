// -----------------------------------------------------------------------------
// <copyright file="SourceTemplateConstructor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Project;

internal class SourceTemplateProjector {
    public IReadOnlyList<(TypeModel, IRenderTemplate)> Project(
        IReadOnlyList<InjectionContextDef> injectionContextDefs,
        GeneratorExecutionContext context
    ) {
        return ExceptionAggregator.Try("constructing source templates.",
            context,
            exceptionAggregator => {
                IReadOnlyList<InjectorDef> injectorDefs = injectionContextDefs
                    .Select(injectionContextDef => injectionContextDef.Injector)
                    .ToImmutableList();
                var injectorDefMap = CreateTypeMap(
                    injectorDefs,
                    injector => injector.InjectorInterfaceType,
                    context);

                return injectionContextDefs.SelectCatching("constructing injection templates",
                        t => t.ToString(),
                        context,
                        injectionContextDef => {
                            var specDefMap = CreateTypeMap(
                                injectionContextDef.SpecContainers,
                                spec => spec.SpecificationType,
                                context);
                            var dependencyDefMap = CreateTypeMap(
                                injectionContextDef.DependencyImplementations,
                                dep => dep.DependencyInterfaceType,
                                context);

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
                                    new InjectorProjector().Project(
                                        injectorDef,
                                        templateGenerationContext)
                                ));
                            context.Log($"Generated injector {injectorDef.InjectorType}.");

                            var specContainerPresenter = new SpecContainerProjector();
                            foreach (var specContainerDef in injectionContextDef.SpecContainers) {
                                templates.Add(
                                    (
                                        specContainerDef.SpecContainerType,
                                        specContainerPresenter.Project(
                                            specContainerDef,
                                            templateGenerationContext)
                                    ));
                                context.Log(
                                    $"Generated spec container {specContainerDef.SpecContainerType} for injector {injectorDef.InjectorType}.");
                            }

                            var dependencyImplementationPresenter
                                = new DependencyImplementationProjector();
                            foreach (var dependency in injectionContextDef
                                .DependencyImplementations) {
                                templates.Add(
                                    (
                                        dependency.DependencyImplementationType,
                                        dependencyImplementationPresenter.Construct(
                                            dependency,
                                            templateGenerationContext)
                                    ));
                                context.Log(
                                    $"Generated dependency implementation {dependency.DependencyImplementationType} for injector {injectorDef.InjectorType}.");
                            }

                            return templates;
                        })
                    .SelectMany(flatten => flatten)
                    .ToImmutableList();
            });
    }

    private static IReadOnlyDictionary<TypeModel, T> CreateTypeMap<T>(
        IEnumerable<T> values,
        Func<T, TypeModel> extractKey,
        GeneratorExecutionContext context
    ) where T : ISourceCodeElement {
        var map = new Dictionary<TypeModel, T>();
        foreach (var value in values) {
            var key = extractKey(value);
            if (map.ContainsKey(key)) {
                throw new InjectionException(
                    $"{typeof(T).Name} with {key} is already defined.",
                    Diagnostics.InvalidSpecification,
                    value.Location,
                    context
                );
            }

            map.Add(key, value);
        }

        return map;
    }
}
