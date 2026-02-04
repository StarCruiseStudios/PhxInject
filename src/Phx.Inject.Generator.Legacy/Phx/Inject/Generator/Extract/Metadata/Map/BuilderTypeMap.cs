// -----------------------------------------------------------------------------
// <copyright file="BuilderTypeMap.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata.Map;

internal record BuilderTypeMap(
    ISet<QualifiedTypeModel> ProvidedBuilderTypes,
    ISet<QualifiedTypeModel> NeededBuilderTypes,
    ISet<QualifiedTypeModel> AutoBuilderEligibleTypes,
    IReadOnlyDictionary<QualifiedTypeModel, IReadOnlyList<TypeModel>> ProvidedBuilderTypeSpecFactoryListMap
) {
    internal interface IExtractor {
        BuilderTypeMap ExtractInjectorBuilderTypeMap(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        );

        BuilderTypeMap Merge(IEnumerable<BuilderTypeMap> builderTypeMaps, ExtractorContext currentCtx);
    }

    internal class Extractor : IExtractor {
        public static readonly IExtractor Instance = new Extractor();

        public BuilderTypeMap ExtractInjectorBuilderTypeMap(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting builder type map for {injectorMetadata.InjectorInterfaceType}",
                injectorMetadata.InjectorInterfaceTypeSymbol,
                currentCtx => {
                    var providedBuildersTypes = new HashSet<QualifiedTypeModel>();
                    var neededBuildersTypes = new HashSet<QualifiedTypeModel>();

                    var providedBuilderSpecTypeMapBuilder = new Dictionary<QualifiedTypeModel, List<TypeModel>>();

                    foreach (var builder in injectorMetadata.Builders) {
                        neededBuildersTypes.Add(builder.BuiltType);
                    }

                    foreach (var spec in injectorSpecMetadataMap.Values) {
                        var specProvidedBuilderTypes = new List<QualifiedTypeModel>();
                        foreach (var builder in spec.Builders) {
                            providedBuildersTypes.Add(builder.BuiltType);
                            specProvidedBuilderTypes.Add(builder.BuiltType);
                        }

                        specProvidedBuilderTypes.ForEach(builtType => {
                            if (!providedBuilderSpecTypeMapBuilder.TryGetValue(builtType, out var specList)) {
                                specList = new List<TypeModel>();
                                providedBuilderSpecTypeMapBuilder[builtType] = specList;
                            }

                            specList.Add(spec.SpecType);
                        });
                    }

                    var autoBuilderEligibleTypes = new HashSet<QualifiedTypeModel>();
                    if (currentCtx.GeneratorSettings.AllowConstructorFactories) {
                        autoBuilderEligibleTypes.UnionWith(neededBuildersTypes);
                        autoBuilderEligibleTypes.ExceptWith(providedBuildersTypes);
                    }

                    var providedBuilderSpecTypeMap = providedBuilderSpecTypeMapBuilder
                        .ToImmutableMultiMap<QualifiedTypeModel, TypeModel, List<TypeModel>>();

                    return new BuilderTypeMap(
                        providedBuildersTypes,
                        neededBuildersTypes,
                        autoBuilderEligibleTypes,
                        providedBuilderSpecTypeMap
                    );
                });
        }

        public BuilderTypeMap Merge(IEnumerable<BuilderTypeMap> builderTypeMaps, ExtractorContext currentCtx) {
            var providedBuilderTypes = new HashSet<QualifiedTypeModel>();
            var neededBuilderTypes = new HashSet<QualifiedTypeModel>();
            var autoBuilderEligibleTypes = new HashSet<QualifiedTypeModel>();
            var providedBuilderTypeSpecFactoryListMap = new Dictionary<QualifiedTypeModel, List<TypeModel>>();

            foreach (var map in builderTypeMaps) {
                providedBuilderTypes.UnionWith(map.ProvidedBuilderTypes);
                neededBuilderTypes.UnionWith(map.NeededBuilderTypes);
                autoBuilderEligibleTypes.UnionWith(map.AutoBuilderEligibleTypes);

                foreach (var kvp in map.ProvidedBuilderTypeSpecFactoryListMap) {
                    if (!providedBuilderTypeSpecFactoryListMap.TryGetValue(kvp.Key, out var specList)) {
                        specList = new List<TypeModel>();
                        providedBuilderTypeSpecFactoryListMap[kvp.Key] = specList;
                    }

                    specList.AddRange(kvp.Value);
                }
            }

            var providedBuilderSpecTypeMap = providedBuilderTypeSpecFactoryListMap
                .ToImmutableMultiMap<QualifiedTypeModel, TypeModel, List<TypeModel>>();

            return new BuilderTypeMap(
                providedBuilderTypes,
                neededBuilderTypes,
                autoBuilderEligibleTypes,
                providedBuilderSpecTypeMap);
        }
    }
}
