// -----------------------------------------------------------------------------
// <copyright file="SourceDefMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Map.Definitions;

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
        SourceMetadata sourceMetadata,
        IGeneratorContext generatorCtx
    ) {
        return ExceptionAggregator.Try(
            "mapping source definition",
            generatorCtx,
            exceptionAggregator => {
                var injectorMetadataMap = CreateTypeMap(
                    sourceMetadata.injectorMetadata,
                    injector => injector.InjectorInterfaceType,
                    generatorCtx);
                var specMetadataMap = CreateTypeMap(
                    sourceMetadata.specMetadata,
                    spec => spec.SpecType,
                    generatorCtx);
                var dependencyMetadataMap = CreateTypeMap(
                    sourceMetadata.dependencyMetadata,
                    dep => dep.DependencyInterfaceType,
                    generatorCtx);

                return sourceMetadata.injectorMetadata.SelectCatching(
                        exceptionAggregator,
                        injectorMetadata => $"extracting injector {injectorMetadata.InjectorInterfaceType}",
                        injectorMetadata => {
                            var injectorSpecMap = new Dictionary<TypeModel, SpecMetadata>();
                            foreach (var specMetadata in injectorMetadata.SpecificationsTypes) {
                                if (!specMetadataMap.TryGetValue(specMetadata, out var spec)) {
                                    throw Diagnostics.IncompleteSpecification.AsException(
                                        $"Cannot find required specification type {spec} while generating injection for type {injectorMetadata.InjectorInterfaceType}.",
                                        injectorMetadata.Location,
                                        generatorCtx);
                                }

                                injectorSpecMap.Add(specMetadata, spec);
                            }

                            var defGenerationCtx = new DefGenerationContext(
                                injectorMetadata,
                                injectorMetadataMap,
                                injectorSpecMap,
                                dependencyMetadataMap,
                                ImmutableDictionary<RegistrationIdentifier, List<FactoryRegistration>>.Empty,
                                ImmutableDictionary<RegistrationIdentifier, BuilderRegistration>.Empty,
                                generatorCtx);

                            if (generatorSettings.AllowConstructorFactories) {
                                var constructorSpec =
                                    specDefMapper.ExtractConstructorSpecForContext(defGenerationCtx);

                                if (constructorSpec != null) {
                                    injectorSpecMap.Add(constructorSpec.SpecType, constructorSpec);
                                }
                            }

                            return new InjectionDefMapper().Map(defGenerationCtx);
                        })
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
                    generatorCtx);
            }

            map.Add(key, value);
        }

        return map;
    }
}
