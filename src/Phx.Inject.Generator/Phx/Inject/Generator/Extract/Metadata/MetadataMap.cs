// -----------------------------------------------------------------------------
// <copyright file="MetadataMap.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record MetadataMap(
    IReadOnlyDictionary<TypeModel, InjectorMetadata> InjectorMetadataMap,
    IReadOnlyDictionary<TypeModel, SpecMetadata> SpecMetadataMap,
    IReadOnlyDictionary<TypeModel, DependencyMetadata> DependencyMetadataMap,
    IReadOnlyDictionary<TypeModel, IReadOnlyDictionary<TypeModel, SpecMetadata>> InjectorSpecMetadataListMap,
    IReadOnlyDictionary<TypeModel, SpecMetadata> AutoFactorySpecMetadataMap
) {
    internal interface IExtractor {
        MetadataMap Extract(
            IReadOnlyList<InjectorMetadata> injectorMetadataList,
            IReadOnlyList<SpecMetadata> specMetadataList,
            IReadOnlyList<DependencyMetadata> dependencyMetadataList,
            IGeneratorContext parentCtx
        );
    }

    internal class Exractor(
        AutoFactoryConstructorMetadata.IExtractor autoFactoryConstructorExtractor,
        SpecLinkMetadata.IExtractor specLinkExtractor,
        SpecMetadata.IAutoSpecExtractor autoSpecExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Exractor(
            AutoFactoryConstructorMetadata.Extractor.Instance,
            SpecLinkMetadata.Extractor.Instance,
            SpecMetadata.AutoSpecExtractor.Instance);

        public MetadataMap Extract(
            IReadOnlyList<InjectorMetadata> injectorMetadataList,
            IReadOnlyList<SpecMetadata> specMetadataList,
            IReadOnlyList<DependencyMetadata> dependencyMetadataList,
            IGeneratorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext<MetadataMap>(
                $"extracting metadata map for assembly {parentCtx.ExecutionContext.Compilation.Assembly}",
                parentCtx.ExecutionContext.Compilation.Assembly,
                currentCtx => {
                    var injectorMetadataMap = CreateTypeMap(
                        injectorMetadataList,
                        injector => injector.InjectorInterfaceType,
                        injector => injector,
                        currentCtx);
                    var specMetadataMap = CreateTypeMap(
                        specMetadataList,
                        spec => spec.SpecType,
                        spec => spec,
                        currentCtx);
                    var dependencyMetadataMap = CreateTypeMap(
                        dependencyMetadataList,
                        dep => dep.DependencyInterfaceType,
                        dep => dep,
                        currentCtx);

                    var injectorSpecMetadataListMap = ImmutableDictionary
                        .CreateBuilder<TypeModel, IReadOnlyDictionary<TypeModel, SpecMetadata>>();
                    var autoFactorySpecMetadataMap = ImmutableDictionary.CreateBuilder<TypeModel, SpecMetadata>();
                    foreach (var injector in injectorMetadataList) {
                        var injectorSpecMetadataMap = CreateTypeMap(
                            injector.SpecificationsTypes,
                            specType => specType,
                            specType => {
                                if (!specMetadataMap.TryGetValue(specType, out var specMetadata)) {
                                    throw Diagnostics.IncompleteSpecification.AsException(
                                        $"Cannot find required specification type {specType} while mapping metadata for {injector.InjectorInterfaceType}.",
                                        injector.Location,
                                        currentCtx);
                                }

                                return specMetadata;
                            },
                            currentCtx);

                        var neededFactoryTypes = ExtractNeededFactoryTypes(
                            injector,
                            injectorSpecMetadataMap,
                            currentCtx);
                        var neededBuilderTypes = ExtractNeededBuilderTypes(
                            injector,
                            injectorSpecMetadataMap,
                            currentCtx);

                        var autoFactorySpec = currentCtx.GeneratorSettings.AllowConstructorFactories
                            && autoSpecExtractor.CanExtract(
                                injector.InjectorType,
                                neededFactoryTypes,
                                neededBuilderTypes)
                                ? autoSpecExtractor.Extract(
                                    injector.InjectorType,
                                    neededFactoryTypes,
                                    neededBuilderTypes,
                                    currentCtx)
                                : null;

                        var injectorSpecList = injectorSpecMetadataMap.Values.ToImmutableList();
                        if (autoFactorySpec != null) {
                            autoFactorySpecMetadataMap[autoFactorySpec.SpecType] = autoFactorySpec;
                            injectorSpecList = injectorSpecList.Add(autoFactorySpec);
                        } else {
                            currentCtx.Aggregator.Aggregate(
                                $"mapping required factory and builder types for {injector.InjectorInterfaceType}",
                                neededFactoryTypes
                                    .Select(factoryType => {
                                        return (Action)(() => throw Diagnostics.IncompleteSpecification.AsException(
                                            $"Cannot find factory {factoryType} while mapping metadata for {injector.InjectorInterfaceType}.",
                                            injector.Location,
                                            currentCtx));
                                    })
                                    .Concat(neededFactoryTypes.Select(builderType => {
                                        return (Action)(() => throw Diagnostics.IncompleteSpecification.AsException(
                                            $"Cannot find builder {builderType} while mapping metadata for {injector.InjectorInterfaceType}.",
                                            injector.Location,
                                            currentCtx));
                                    })));
                        }

                        injectorSpecMetadataListMap[injector.InjectorInterfaceType] = CreateTypeMap(
                            injectorSpecList,
                            spec => spec.SpecType,
                            spec => spec,
                            currentCtx);
                    }

                    return new MetadataMap(
                        injectorMetadataMap,
                        specMetadataMap,
                        dependencyMetadataMap,
                        injectorSpecMetadataListMap.ToImmutableDictionary(),
                        autoFactorySpecMetadataMap.ToImmutableDictionary()
                    );
                });
        }

        private ISet<QualifiedTypeModel> ExtractNeededFactoryTypes(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting needed factory types for {injectorMetadata.InjectorInterfaceType}",
                injectorMetadata.InjectorInterfaceTypeSymbol,
                currentCtx => {
                    var providedTypes = new HashSet<QualifiedTypeModel>();
                    var neededTypes = new HashSet<QualifiedTypeModel>();

                    foreach (var provider in injectorMetadata.Providers) {
                        if (provider.ProvidedType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
                            neededTypes.Add(
                                provider.ProvidedType.TypeModel.TypeArguments[0]
                                    .TypeSymbol
                                    .ToQualifiedTypeModel(provider.ProvidedType.Qualifier));
                        } else {
                            neededTypes.Add(provider.ProvidedType);
                        }
                    }

                    foreach (var spec in injectorSpecMetadataMap.Values) {
                        foreach (var factory in spec.Factories) {
                            providedTypes.Add(factory.ReturnType);

                            foreach (var parameterType in factory.Parameters) {
                                if (parameterType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
                                    var factoryType = parameterType with {
                                        TypeModel = parameterType.TypeModel.TypeArguments.Single()
                                    };
                                    neededTypes.Add(factoryType);
                                } else {
                                    neededTypes.Add(parameterType);
                                }
                            }
                        }

                        foreach (var link in spec.Links) {
                            providedTypes.Add(link.ReturnType);
                            neededTypes.Add(link.InputType);
                        }

                        foreach (var builder in spec.Builders) {
                            foreach (var parameterType in builder.Parameters) {
                                neededTypes.Add(parameterType);
                            }
                        }
                    }

                    if (currentCtx.GeneratorSettings.AllowConstructorFactories) {
                        var typeSearchQueue = new Queue<QualifiedTypeModel>();
                        var autoFactoryTypes = new HashSet<QualifiedTypeModel>();
                        foreach (var qualifiedTypeModel in neededTypes) {
                            typeSearchQueue.Enqueue(qualifiedTypeModel);
                        }

                        while (typeSearchQueue.Count > 0) {
                            var type = typeSearchQueue.Dequeue();
                            if (!providedTypes.Contains(type) && !autoFactoryTypes.Contains(type)) {
                                if (autoFactoryConstructorExtractor.CanExtract(type.TypeModel.TypeSymbol)) {
                                    var constructor =
                                        autoFactoryConstructorExtractor.Extract(type.TypeModel.TypeSymbol, currentCtx);
                                    foreach (var parameterType in constructor.ParameterTypes) {
                                        if (neededTypes.Add(parameterType)) {
                                            typeSearchQueue.Enqueue(parameterType);
                                        }
                                    }

                                    foreach (var requiredProperty in constructor.RequiredProperties) {
                                        if (neededTypes.Add(requiredProperty.PropertyType)) {
                                            typeSearchQueue.Enqueue(requiredProperty.PropertyType);
                                        }
                                    }

                                    var links = specLinkExtractor.CanExtract(type.TypeModel)
                                        ? specLinkExtractor.ExtractAll(type.TypeModel, currentCtx)
                                            .ToImmutableList()
                                        : ImmutableList<SpecLinkMetadata>.Empty;
                                    foreach (var link in links) {
                                        if (neededTypes.Add(link.InputType)) {
                                            typeSearchQueue.Enqueue(link.InputType);
                                        }

                                        providedTypes.Add(link.ReturnType);
                                    }

                                    autoFactoryTypes.Add(type);
                                }
                            }
                        }
                    }

                    return neededTypes
                        .Except(providedTypes)
                        .ToImmutableHashSet();
                });
        }

        private ISet<QualifiedTypeModel> ExtractNeededBuilderTypes(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting needed factory types for {injectorMetadata.InjectorInterfaceType}",
                injectorMetadata.InjectorInterfaceTypeSymbol,
                currentCtx => {
                    var providedBuilders = new HashSet<QualifiedTypeModel>();
                    var neededBuilders = new HashSet<QualifiedTypeModel>();

                    foreach (var builder in injectorMetadata.Builders) {
                        neededBuilders.Add(builder.BuiltType);
                    }

                    foreach (var spec in injectorSpecMetadataMap.Values) {
                        foreach (var builder in spec.Builders) {
                            providedBuilders.Add(builder.BuiltType);
                        }
                    }

                    return neededBuilders
                        .Except(providedBuilders)
                        .ToImmutableHashSet();
                });
        }

        private IReadOnlyDictionary<TypeModel, R> CreateTypeMap<T, R>(
            IEnumerable<T> elements,
            Func<T, TypeModel> extractKey,
            Func<T, R> extractValue,
            IGeneratorContext generatorCtx
        ) where R : ISourceCodeElement {
            var map = ImmutableDictionary.CreateBuilder<TypeModel, R>();
            elements.SelectCatching(
                    generatorCtx.Aggregator,
                    element => $"Mapping metadata for element {element}",
                    element => {
                        var key = extractKey(element);
                        if (map.ContainsKey(key)) {
                            throw Diagnostics.InternalError.AsException(
                                $"Duplicate metadata for type {typeof(T).Name}.",
                                key.Location,
                                generatorCtx);
                        }

                        var value = extractValue(element);
                        map.Add(key, value);
                        return (key, value);
                    })
                .ToImmutableList();

            return map.ToImmutableDictionary();
        }
    }
}
