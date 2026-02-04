// -----------------------------------------------------------------------------
// <copyright file="FactoryTypeMap.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata.Map;

internal record FactoryTypeMap(
    ISet<QualifiedTypeModel> ProvidedFactoryTypes,
    ISet<QualifiedTypeModel> NeededFactoryTypes,
    ISet<QualifiedTypeModel> AutoFactoryEligibleTypes,
    IReadOnlyDictionary<QualifiedTypeModel, IReadOnlyList<TypeModel>> ProvidedFactoryTypeSpecFactoryListMap
) {
    internal interface IExtractor {
        FactoryTypeMap ExtractInjectorFactoryTypeMap(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        );

        FactoryTypeMap Merge(IEnumerable<FactoryTypeMap> factoryTypeMaps, ExtractorContext currentCtx);
    }

    internal class Extractor(
        AutoFactoryConstructorMetadata.IExtractor autoFactoryConstructorExtractor,
        SpecLinkMetadata.IExtractor specLinkExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            AutoFactoryConstructorMetadata.Extractor.Instance,
            SpecLinkMetadata.Extractor.Instance);

        public FactoryTypeMap ExtractInjectorFactoryTypeMap(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting factory type map for {injectorMetadata.InjectorInterfaceType}",
                injectorMetadata.InjectorInterfaceTypeSymbol,
                currentCtx => {
                    var providedFactoryTypes = new HashSet<QualifiedTypeModel>();
                    var neededFactoryTypes = new HashSet<QualifiedTypeModel>();

                    var providedFactorySpecTypeMapBuilder = new Dictionary<QualifiedTypeModel, List<TypeModel>>();

                    foreach (var provider in injectorMetadata.Providers) {
                        if (provider.ProvidedType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
                            neededFactoryTypes.Add(
                                provider.ProvidedType.TypeModel.TypeArguments[0]
                                    .TypeSymbol
                                    .ToQualifiedTypeModel(provider.ProvidedType.Qualifier));
                        } else {
                            neededFactoryTypes.Add(provider.ProvidedType);
                        }
                    }

                    foreach (var spec in injectorSpecMetadataMap.Values) {
                        var specProvidedFactoryTypes = new List<QualifiedTypeModel>();
                        foreach (var factory in spec.Factories) {
                            providedFactoryTypes.Add(factory.ReturnType);
                            specProvidedFactoryTypes.Add(factory.ReturnType);

                            foreach (var parameterType in factory.Parameters) {
                                if (parameterType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
                                    var factoryType = parameterType with {
                                        TypeModel = parameterType.TypeModel.TypeArguments.Single()
                                    };
                                    neededFactoryTypes.Add(factoryType);
                                } else {
                                    neededFactoryTypes.Add(parameterType);
                                }
                            }
                        }

                        foreach (var link in spec.Links) {
                            providedFactoryTypes.Add(link.ReturnType);
                            specProvidedFactoryTypes.Add(link.ReturnType);
                            neededFactoryTypes.Add(link.InputType);
                        }

                        foreach (var builder in spec.Builders) {
                            foreach (var parameterType in builder.Parameters) {
                                neededFactoryTypes.Add(parameterType);
                            }
                        }

                        specProvidedFactoryTypes.ForEach(providedType => {
                            if (!providedFactorySpecTypeMapBuilder.TryGetValue(providedType, out var specList)) {
                                specList = new List<TypeModel>();
                                providedFactorySpecTypeMapBuilder[providedType] = specList;
                            }

                            specList.Add(spec.SpecType);
                        });
                    }

                    var autoFactoryEligibleTypes = new HashSet<QualifiedTypeModel>();
                    if (currentCtx.GeneratorSettings.AllowConstructorFactories) {
                        var typeSearchQueue = new Queue<QualifiedTypeModel>();
                        foreach (var qualifiedTypeModel in neededFactoryTypes) {
                            typeSearchQueue.Enqueue(qualifiedTypeModel);
                        }

                        while (typeSearchQueue.Count > 0) {
                            var type = typeSearchQueue.Dequeue();
                            if (!providedFactoryTypes.Contains(type) && !autoFactoryEligibleTypes.Contains(type)) {
                                if (autoFactoryConstructorExtractor.CanExtract(type.TypeModel.TypeSymbol)) {
                                    var constructor =
                                        autoFactoryConstructorExtractor.Extract(type.TypeModel.TypeSymbol, currentCtx);
                                    foreach (var parameterType in constructor.ParameterTypes) {
                                        if (neededFactoryTypes.Add(parameterType)) {
                                            typeSearchQueue.Enqueue(parameterType);
                                        }
                                    }

                                    foreach (var requiredProperty in constructor.RequiredProperties) {
                                        if (neededFactoryTypes.Add(requiredProperty.PropertyType)) {
                                            typeSearchQueue.Enqueue(requiredProperty.PropertyType);
                                        }
                                    }

                                    var links = specLinkExtractor.CanExtract(type.TypeModel)
                                        ? specLinkExtractor.ExtractAll(type.TypeModel, currentCtx)
                                            .ToImmutableList()
                                        : ImmutableList<SpecLinkMetadata>.Empty;
                                    foreach (var link in links) {
                                        if (neededFactoryTypes.Add(link.InputType)) {
                                            typeSearchQueue.Enqueue(link.InputType);
                                        }

                                        providedFactoryTypes.Add(link.ReturnType);
                                    }

                                    autoFactoryEligibleTypes.Add(type);
                                }
                            }
                        }
                    }

                    var providedFactorySpecTypeMap = providedFactorySpecTypeMapBuilder
                        .ToImmutableMultiMap<QualifiedTypeModel, TypeModel, List<TypeModel>>();

                    return new FactoryTypeMap(
                        providedFactoryTypes,
                        neededFactoryTypes,
                        autoFactoryEligibleTypes,
                        providedFactorySpecTypeMap
                    );
                });
        }

        public FactoryTypeMap Merge(IEnumerable<FactoryTypeMap> factoryTypeMaps, ExtractorContext currentCtx) {
            var providedFactoryTypes = new HashSet<QualifiedTypeModel>();
            var neededFactoryTypes = new HashSet<QualifiedTypeModel>();
            var autoFactoryEligibleTypes = new HashSet<QualifiedTypeModel>();
            var providedFactorySpecTypeMapBuilder = new Dictionary<QualifiedTypeModel, List<TypeModel>>();

            foreach (var factoryTypeMap in factoryTypeMaps) {
                providedFactoryTypes.UnionWith(factoryTypeMap.ProvidedFactoryTypes);
                neededFactoryTypes.UnionWith(factoryTypeMap.NeededFactoryTypes);
                autoFactoryEligibleTypes.UnionWith(factoryTypeMap.AutoFactoryEligibleTypes);

                foreach (var kvp in factoryTypeMap.ProvidedFactoryTypeSpecFactoryListMap) {
                    if (!providedFactorySpecTypeMapBuilder.TryGetValue(kvp.Key, out var specList)) {
                        specList = new List<TypeModel>();
                        providedFactorySpecTypeMapBuilder[kvp.Key] = specList;
                    }

                    specList.AddRange(kvp.Value);
                }
            }

            var providedFactorySpecTypeMap = providedFactorySpecTypeMapBuilder
                .ToImmutableMultiMap<QualifiedTypeModel, TypeModel, List<TypeModel>>();

            return new FactoryTypeMap(
                providedFactoryTypes,
                neededFactoryTypes,
                autoFactoryEligibleTypes,
                providedFactorySpecTypeMap);
        }
    }
}
