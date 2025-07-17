// -----------------------------------------------------------------------------
// <copyright file="SourceTemplateConstructor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Project;

internal class SourceTemplateProjector {
    public IReadOnlyList<(TypeModel, IRenderTemplate)> Project(
        IReadOnlyList<InjectionContextDef> injectionContextDefs,
        IGeneratorContext parentCtx
    ) {
        return ExceptionAggregator.Try(
            "constructing source templates.",
            parentCtx,
            exceptionAggregator => {
                IReadOnlyList<InjectorDef> injectorDefs = injectionContextDefs
                    .Select(injectionContextDef => injectionContextDef.Injector)
                    .ToImmutableList();
                var injectorDefMap = CreateTypeMap(
                    injectorDefs,
                    injector => injector.InjectorInterfaceType,
                    parentCtx);

                return injectionContextDefs.SelectCatching(
                        exceptionAggregator,
                        injectionContextDef =>
                            $"constructing injection templates for {injectionContextDef.Injector.InjectorInterfaceType}.",
                        injectionContextDef => {
                            var specDefMap = CreateTypeMap(
                                injectionContextDef.SpecContainers,
                                spec => spec.SpecificationType,
                                parentCtx);
                            var dependencyDefMap = CreateTypeMap(
                                injectionContextDef.DependencyImplementations,
                                dep => dep.DependencyInterfaceType,
                                parentCtx);

                            var templateGenerationContext = new TemplateGenerationContext(
                                injectionContextDef.Injector,
                                injectorDefMap,
                                specDefMap,
                                dependencyDefMap,
                                parentCtx.ExecutionContext.Compilation.Assembly,
                                parentCtx);

                            var templates = new List<(TypeModel, IRenderTemplate)>();
                            var injectorDef = injectionContextDef.Injector;
                            templates.Add(
                                (
                                    injectorDef.InjectorType,
                                    new InjectorProjector().Project(
                                        injectorDef,
                                        templateGenerationContext)
                                ));
                            parentCtx.Log($"Generated injector {injectorDef.InjectorType}.");

                            var specContainerPresenter = new SpecContainerProjector();
                            foreach (var specContainerDef in injectionContextDef.SpecContainers) {
                                templates.Add(
                                    (
                                        specContainerDef.SpecContainerType,
                                        specContainerPresenter.Project(
                                            specContainerDef,
                                            templateGenerationContext)
                                    ));
                                parentCtx.Log(
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
                                parentCtx.Log(
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
        IGeneratorContext generatorCtx
    ) where T : ISourceCodeElement {
        var map = new Dictionary<TypeModel, T>();
        foreach (var value in values) {
            var key = extractKey(value);
            if (map.ContainsKey(key)) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"{typeof(T).Name} with {key} is already defined.",
                    value.Location,
                    generatorCtx
                );
            }

            map.Add(key, value);
        }

        return map;
    }
}
