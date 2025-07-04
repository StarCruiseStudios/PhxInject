// -----------------------------------------------------------------------------
// <copyright file="SourceDefMapper.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Generator.Map;

internal class SourceDefMapper {
    private readonly GeneratorSettings generatorSettings;
    private readonly ISpecDefMapper specDefMapper;

    public SourceDefMapper(ISpecDefMapper specDefMapper, GeneratorSettings generatorSettings) {
        this.specDefMapper = specDefMapper;
        this.generatorSettings = generatorSettings;
    }

    public SourceDefMapper(GeneratorSettings generatorSettings)
        : this(new SpecDefMapper(), generatorSettings) { }

    public IReadOnlyList<InjectionContextDef> Map(
        SourceDesc sourceDesc,
        GeneratorExecutionContext context
    ) {
        return ExceptionAggregator.Try(
            "mapping source definition",
            Location.None,
            context,
            exceptionAggregator => {
                var injectorDescMap = CreateTypeMap(
                    sourceDesc.injectorDescs,
                    injector => injector.InjectorInterfaceType,
                    context);
                var specDescMap = CreateTypeMap(
                    sourceDesc.GetAllSpecDescs(),
                    spec => spec.SpecType,
                    context);
                var dependencyDescMap = CreateTypeMap(
                    sourceDesc.dependencyDescs,
                    dep => dep.DependencyInterfaceType,
                    context);

                return sourceDesc.injectorDescs.SelectCatching(
                        exceptionAggregator,
                        injectorDesc => $"extracting injector {injectorDesc.InjectorInterfaceType}",
                        injectorDesc => {
                            var injectorSpecDescMap = new Dictionary<TypeModel, SpecDesc>();
                            foreach (var spec in injectorDesc.SpecificationsTypes) {
                                if (!specDescMap.TryGetValue(spec, out var specDesc)) {
                                    throw Diagnostics.IncompleteSpecification.AsException(
                                        $"Cannot find required specification type {spec} while generating injection for type {injectorDesc.InjectorInterfaceType}.",
                                        injectorDesc.Location,
                                        context);
                                }

                                injectorSpecDescMap.Add(spec, specDesc);
                            }

                            if (generatorSettings.AllowConstructorFactories) {
                                var constructorSpec = specDefMapper.ExtractConstructorSpecForContext(
                                    new DefGenerationContext(
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
                throw Diagnostics.InvalidSpecification.AsException(
                    $"{typeof(T).Name} with {key} is already defined.",
                    value.Location,
                    context);
            }

            map.Add(key, value);
        }

        return map;
    }
}
