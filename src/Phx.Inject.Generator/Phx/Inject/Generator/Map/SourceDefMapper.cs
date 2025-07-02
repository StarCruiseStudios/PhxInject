// -----------------------------------------------------------------------------
// <copyright file="SourceDefMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract;
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Generator.Map;

internal class SourceDefMapper {
    private readonly ISpecExtractor specExtractor;
    private readonly GeneratorSettings generatorSettings;
    
    public SourceDefMapper(ISpecExtractor specExtractor, GeneratorSettings generatorSettings) {
        this.specExtractor = specExtractor;
        this.generatorSettings = generatorSettings;
    }
    
    public SourceDefMapper(GeneratorSettings generatorSettings)
        : this(new SpecExtractor(), generatorSettings) { }

    public IReadOnlyList<InjectionContextDef> Map(
        SourceDesc sourceDesc,
        GeneratorExecutionContext context
    ) {
        return InjectionException.Try(context, () => {
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

                return sourceDesc.injectorDescs.SelectCatching(context, injectorDesc => {
                        var injectorSpecDescMap = new Dictionary<TypeModel, SpecDesc>();
                        foreach (var spec in injectorDesc.SpecificationsTypes) {
                            if (!specDescMap.TryGetValue(spec, out var specDesc)) {
                                throw new InjectionException(
                                    context,
                                    Diagnostics.IncompleteSpecification,
                                    $"Cannot find required specification type {spec}"
                                    + $" while generating injection for type {injectorDesc.InjectorInterfaceType}.",
                                    injectorDesc.Location);
                            }

                            injectorSpecDescMap.Add(spec, specDesc);
                        }

                        if (generatorSettings.AllowConstructorFactories) {
                            var constructorSpec = specExtractor.ExtractConstructorSpecForContext(
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
            },
            "mapping source definition");
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
                    context,
                    Diagnostics.InvalidSpecification,
                    $"{typeof(T).Name} with {key} is already defined.",
                    value.Location);
            }

            map.Add(key, value);
        }

        return map;
    }
}
