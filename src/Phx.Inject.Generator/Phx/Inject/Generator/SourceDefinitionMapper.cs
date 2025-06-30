// -----------------------------------------------------------------------------
// <copyright file="SourceDefMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Common;
using Phx.Inject.Generator.Definitions;
using Phx.Inject.Generator.Descriptors;
using Phx.Inject.Generator.Model;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Generator;

internal class SourceDefMapper {
    private readonly GeneratorSettings generatorSettings;
    public SourceDefMapper(GeneratorSettings generatorSettings) {
        this.generatorSettings = generatorSettings;
    }

    public IReadOnlyList<InjectionContextDef> MapInjectionContexts(
        SourceDesc sourceDesc,
        GeneratorExecutionContext context
    ) {
        try {
            var injectorDescMap = CreateTypeMap(
                sourceDesc.injectorDescs,
                injector => injector.InjectorInterfaceType);
            var specDescMap = CreateTypeMap(
                sourceDesc.GetAllSpecDescs(),
                spec => spec.SpecType);
            var dependencyDescMap = CreateTypeMap(
                sourceDesc.dependencyDescs,
                dep => dep.DependencyInterfaceType);

            return sourceDesc.injectorDescs.Select(injectorDesc => {
                    var injectorSpecDescMap = new Dictionary<TypeModel, SpecDesc>();
                    foreach (var spec in injectorDesc.SpecificationsTypes) {
                        if (!specDescMap.TryGetValue(spec, out var specDesc)) {
                            throw new InjectionException(
                                Diagnostics.IncompleteSpecification,
                                $"Cannot find required specification type {spec}"
                                + $" while generating injection for type {injectorDesc.InjectorInterfaceType}.",
                                injectorDesc.Location);
                        }

                        injectorSpecDescMap.Add(spec, specDesc);
                    }

                    if (generatorSettings.AllowConstructorFactories) {
                        var constructorSpec = new SpecExtractor()
                            .ExtractConstructorSpecForContext(new DefGenerationContext(
                                injectorDesc,
                                injectorDescMap,
                                injectorSpecDescMap,
                                dependencyDescMap,
                                ImmutableDictionary<RegistrationIdentifier, List<FactoryRegistration>>.Empty,
                                ImmutableDictionary<RegistrationIdentifier, BuilderRegistration>.Empty,
                                context));

                        if (constructorSpec != null) {
                            injectorSpecDescMap.Add(constructorSpec.SpecType, constructorSpec);
                        }
                    }

                    var defGenerationContext = new DefGenerationContext(
                        injectorDesc,
                        injectorDescMap,
                        injectorSpecDescMap,
                        dependencyDescMap,
                        ImmutableDictionary<RegistrationIdentifier, List<FactoryRegistration>>.Empty,
                        ImmutableDictionary<RegistrationIdentifier, BuilderRegistration>.Empty,
                        context);

                    return new InjectionDefMapper().Map(defGenerationContext);
                })
                .ToImmutableList();
        } catch (Exception e) {
            var diagnosticData = (e is InjectionException ie)
                ? ie.DiagnosticData
                : Diagnostics.UnexpectedError;
            
            throw new InjectionException(
                diagnosticData,
                $"An error occurred while mapping source definitions.",
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
