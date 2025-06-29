// -----------------------------------------------------------------------------
// <copyright file="SourceDefinitionMapper.cs" company="Star Cruise Studios LLC">
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

internal class SourceDefinitionMapper {
    private readonly GeneratorSettings generatorSettings;
    public SourceDefinitionMapper(GeneratorSettings generatorSettings) {
        this.generatorSettings = generatorSettings;
    }

    public IReadOnlyList<InjectionContextDefinition> MapInjectionContexts(
        SourceDescriptor sourceDescriptor,
        GeneratorExecutionContext context
    ) {
        try {
            var injectorDescriptorMap = CreateTypeMap(
                sourceDescriptor.injectorDescriptors,
                injector => injector.InjectorInterfaceType);
            var specDescriptorMap = CreateTypeMap(
                sourceDescriptor.GetAllSpecDescriptors(),
                spec => spec.SpecType);
            var externalDependencyDescriptorMap = CreateTypeMap(
                sourceDescriptor.externalDependencyDescriptors,
                dep => dep.ExternalDependencyInterfaceType);

            return sourceDescriptor.injectorDescriptors.Select(injectorDescriptor => {
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
                        var constructorSpec = new SpecExtractor()
                            .ExtractConstructorSpecForContext(new DefinitionGenerationContext(
                                injectorDescriptor,
                                injectorDescriptorMap,
                                injectorSpecDescriptorMap,
                                externalDependencyDescriptorMap,
                                ImmutableDictionary<RegistrationIdentifier, List<FactoryRegistration>>.Empty,
                                ImmutableDictionary<RegistrationIdentifier, BuilderRegistration>.Empty,
                                context));

                        if (constructorSpec != null) {
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
