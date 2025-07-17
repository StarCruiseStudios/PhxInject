// -----------------------------------------------------------------------------
// <copyright file="GlobalTypeMap.cs" company="Star Cruise Studios LLC">
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

internal record GlobalTypeMap(
    FactoryTypeMap FactoryTypeMap,
    BuilderTypeMap BuilderTypeMap,
    IDictionary<TypeModel, InjectorTypeMap> InjectorTypeMaps
) {
    internal interface IExtractor {
        GlobalTypeMap Extract(
            IReadOnlyList<InjectorMetadata> injectorMetadataList,
            IReadOnlyDictionary<TypeModel, SpecMetadata> specMetadataMap,
            IGeneratorContext parentCtx
        );
    }

    internal class Extractor(
        InjectorTypeMap.IExtractor injectorTypeMapExtractor,
        FactoryTypeMap.IExtractor factoryTypeMapExtractor,
        BuilderTypeMap.IExtractor builderTypeMapExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            InjectorTypeMap.Extractor.Instance,
            FactoryTypeMap.Extractor.Instance,
            BuilderTypeMap.Extractor.Instance
        );

        public GlobalTypeMap Extract(
            IReadOnlyList<InjectorMetadata> injectorMetadataList,
            IReadOnlyDictionary<TypeModel, SpecMetadata> specMetadataMap,
            IGeneratorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting metadata map for assembly {parentCtx.ExecutionContext.Compilation.Assembly}",
                parentCtx.ExecutionContext.Compilation.Assembly,
                currentCtx => {
                    var injectorTypeMapsBuilder = ImmutableDictionary.CreateBuilder<TypeModel, InjectorTypeMap>();
                    foreach (var injector in injectorMetadataList) {
                        var injectorSpecMetadataMap = injector.SpecificationsTypes.CreateTypeMap(
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

                        var injectorTypeMap = injectorTypeMapExtractor.ExtractInjectorTypeMap(
                            injector,
                            injectorSpecMetadataMap,
                            currentCtx);
                        injectorTypeMapsBuilder.Add(injectorTypeMap.InjectorType, injectorTypeMap);
                    }

                    var injectorTypeMaps = injectorTypeMapsBuilder.ToImmutable();

                    var globalFactoryMap = factoryTypeMapExtractor.Merge(
                        injectorTypeMaps.Values.Select(injectorTypeMap => injectorTypeMap.InjectorFactoryTypeMap),
                        currentCtx);
                    var globalBuilderMap = builderTypeMapExtractor.Merge(
                        injectorTypeMaps.Values.Select(injectorTypeMap => injectorTypeMap.InjectorBuilderTypeMap),
                        currentCtx);

                    return new GlobalTypeMap(
                        globalFactoryMap,
                        globalBuilderMap,
                        injectorTypeMaps);
                });
        }
    }
}
