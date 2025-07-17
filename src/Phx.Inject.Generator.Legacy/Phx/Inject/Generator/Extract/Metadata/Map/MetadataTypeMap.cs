// -----------------------------------------------------------------------------
// <copyright file="MetadataMap.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata.Map;

internal record MetadataTypeMap(
    IReadOnlyDictionary<TypeModel, InjectorMetadata> InjectorMetadataMap,
    IReadOnlyDictionary<TypeModel, SpecMetadata> SpecMetadataMap,
    IReadOnlyDictionary<TypeModel, DependencyMetadata> DependencyMetadataMap,
    IReadOnlyDictionary<TypeModel, IReadOnlyDictionary<TypeModel, SpecMetadata>> InjectorSpecMetadataListMap,
    IReadOnlyDictionary<TypeModel, SpecMetadata> AutoFactorySpecMetadataMap,
    GlobalTypeMap RawMetadataTypeMap
) {
    internal interface IExtractor {
        MetadataTypeMap Extract(
            IReadOnlyList<InjectorMetadata> injectorMetadataList,
            IReadOnlyList<SpecMetadata> specMetadataList,
            IReadOnlyList<DependencyMetadata> dependencyMetadataList,
            IGeneratorContext parentCtx
        );
    }

    internal class Extractor(
        GlobalTypeMap.IExtractor globalTypeMapExtractor,
        SpecMetadata.IAutoSpecExtractor autoSpecExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            GlobalTypeMap.Extractor.Instance,
            SpecMetadata.AutoSpecExtractor.Instance);

        public MetadataTypeMap Extract(
            IReadOnlyList<InjectorMetadata> injectorMetadataList,
            IReadOnlyList<SpecMetadata> specMetadataList,
            IReadOnlyList<DependencyMetadata> dependencyMetadataList,
            IGeneratorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting metadata map for assembly {parentCtx.ExecutionContext.Compilation.Assembly}",
                parentCtx.ExecutionContext.Compilation.Assembly,
                currentCtx => {
                    var injectorMetadataMap = injectorMetadataList.CreateTypeMap(
                        injector => injector.InjectorInterfaceType,
                        injector => injector,
                        currentCtx);
                    var specMetadataMap = specMetadataList.CreateTypeMap(
                        spec => spec.SpecType,
                        spec => spec,
                        currentCtx);
                    var dependencyMetadataMap = dependencyMetadataList.CreateTypeMap(
                        dep => dep.DependencyInterfaceType,
                        dep => dep,
                        currentCtx);

                    var globalTypeMap =
                        globalTypeMapExtractor.Extract(injectorMetadataList, specMetadataMap, parentCtx);

                    var injectorSpecMetadataListMapBuilder = ImmutableDictionary
                        .CreateBuilder<TypeModel, IReadOnlyDictionary<TypeModel, SpecMetadata>>();
                    var autoFactorySpecMetadataMap = ImmutableDictionary.CreateBuilder<TypeModel, SpecMetadata>();
                    foreach (var injectorTypeMap in globalTypeMap.InjectorTypeMaps.Values) {
                        var autoFactoryTypes = injectorTypeMap.InjectorFactoryTypeMap.AutoFactoryEligibleTypes;
                        var autoBuilderTypes = injectorTypeMap.InjectorBuilderTypeMap.AutoBuilderEligibleTypes;
                        var autoFactorySpec = currentCtx.GeneratorSettings.AllowConstructorFactories
                            && autoSpecExtractor.CanExtract(
                                injectorTypeMap.InjectorType,
                                autoFactoryTypes,
                                autoBuilderTypes)
                                ? autoSpecExtractor.Extract(
                                    injectorTypeMap.InjectorType,
                                    autoFactoryTypes,
                                    autoBuilderTypes,
                                    currentCtx)
                                : null;

                        var injectorSpecMetadataMapBuilder =
                            ImmutableDictionary.CreateBuilder<TypeModel, SpecMetadata>();
                        injectorSpecMetadataMapBuilder.AddRange(injectorTypeMap.InjectorSpecMetadataMap);
                        if (autoFactorySpec != null) {
                            autoFactorySpecMetadataMap[autoFactorySpec.SpecType] = autoFactorySpec;
                            injectorSpecMetadataMapBuilder.Add(autoFactorySpec.SpecType, autoFactorySpec);

                            foreach (var factory in autoFactorySpec.Factories) {
                                if (globalTypeMap.FactoryTypeMap.ProvidedFactoryTypes.Contains(factory.ReturnType)) {
                                    var availableSpecifications = globalTypeMap.FactoryTypeMap
                                        .ProvidedFactoryTypeSpecFactoryListMap.TryGetValue(factory.ReturnType,
                                            out var specList)
                                        ? $"\nAvailable in:\n- {string.Join("\n- ", specList)}."
                                        : "";
                                    Diagnostics.AutoFactoryWithSpecification.AsWarning(
                                        $"Injector {injectorTypeMap.InjectorType} is using an auto factory for type {factory.ReturnType} that has an explicit factory in a specification not included by the injector.{availableSpecifications}",
                                        injectorTypeMap.InjectorType.Location,
                                        currentCtx);
                                }
                            }

                            foreach (var builder in autoFactorySpec.Builders) {
                                if (globalTypeMap.BuilderTypeMap.ProvidedBuilderTypes.Contains(builder.BuiltType)) {
                                    var availableSpecifications = globalTypeMap.BuilderTypeMap
                                        .ProvidedBuilderTypeSpecFactoryListMap.TryGetValue(builder.BuiltType,
                                            out var specList)
                                        ? $"\nAvailable in:\n- {string.Join("\n- ", specList)}."
                                        : "";
                                    Diagnostics.AutoFactoryWithSpecification.AsWarning(
                                        $"Injector {injectorTypeMap.InjectorType} is using an auto builder for type {builder.BuiltType} that has an explicit builder in a specification not included by the injector.{availableSpecifications}",
                                        injectorTypeMap.InjectorType.Location,
                                        currentCtx);
                                }
                            }
                        } else {
                            currentCtx.Aggregator.Aggregate(
                                $"mapping required factory and builder types for {injectorTypeMap.InjectorType}",
                                autoFactoryTypes
                                    .Select(factoryType => {
                                        return (Action)(() => throw Diagnostics.IncompleteSpecification.AsException(
                                            $"Cannot find factory {factoryType} while mapping metadata for {injectorTypeMap.InjectorType}.",
                                            injectorTypeMap.InjectorType.Location,
                                            currentCtx));
                                    })
                                    .Concat(autoFactoryTypes.Select(builderType => {
                                        return (Action)(() => throw Diagnostics.IncompleteSpecification.AsException(
                                            $"Cannot find builder {builderType} while mapping metadata for {injectorTypeMap.InjectorType}.",
                                            injectorTypeMap.InjectorType.Location,
                                            currentCtx));
                                    })));
                        }

                        injectorSpecMetadataListMapBuilder.Add(
                            injectorTypeMap.InjectorType,
                            injectorSpecMetadataMapBuilder.ToImmutable());
                    }

                    return new MetadataTypeMap(
                        injectorMetadataMap,
                        specMetadataMap,
                        dependencyMetadataMap,
                        injectorSpecMetadataListMapBuilder.ToImmutable(),
                        autoFactorySpecMetadataMap.ToImmutable(),
                        globalTypeMap
                    );
                });
        }
    }
}
