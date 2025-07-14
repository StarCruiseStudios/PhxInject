// -----------------------------------------------------------------------------
// <copyright file="InjectorTypeMap.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Metadata.Map;

internal record InjectorTypeMap(
    TypeModel InjectorType,
    IReadOnlyDictionary<TypeModel, SpecMetadata> InjectorSpecMetadataMap,
    FactoryTypeMap InjectorFactoryTypeMap,
    BuilderTypeMap InjectorBuilderTypeMap
) {
    internal interface IExtractor {
        InjectorTypeMap ExtractInjectorTypeMap(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        );
    }

    internal class Extractor(
        FactoryTypeMap.IExtractor factoryTypeMapExtractor,
        BuilderTypeMap.IExtractor builderTypeMapExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            FactoryTypeMap.Extractor.Instance,
            BuilderTypeMap.Extractor.Instance);

        public InjectorTypeMap ExtractInjectorTypeMap(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, SpecMetadata> injectorSpecMetadataMap,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting injector type map for {injectorMetadata.InjectorInterfaceType}",
                injectorMetadata.InjectorInterfaceTypeSymbol,
                currentCtx => {
                    var injectorFactoryTypeMap =
                        factoryTypeMapExtractor.ExtractInjectorFactoryTypeMap(
                            injectorMetadata,
                            injectorSpecMetadataMap,
                            currentCtx);
                    var injectorBuilderTypeMap =
                        builderTypeMapExtractor.ExtractInjectorBuilderTypeMap(
                            injectorMetadata,
                            injectorSpecMetadataMap,
                            currentCtx);

                    return new InjectorTypeMap(
                        injectorMetadata.InjectorInterfaceType,
                        injectorSpecMetadataMap,
                        injectorFactoryTypeMap,
                        injectorBuilderTypeMap);
                });
        }
    }
}
