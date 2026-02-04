// -----------------------------------------------------------------------------
// <copyright file="SourceDefMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Map;

internal class SourceDefMapper(InjectionContextDef.IMapper injectionContextDefMapper) {
    public static readonly SourceDefMapper Instance = new(new InjectionContextDef.Mapper());
    public IReadOnlyList<InjectionContextDef> Map(
        SourceMetadata sourceMetadata,
        IGeneratorContext generatorCtx
    ) {
        return ExceptionAggregator.Try(
            "mapping source definition",
            generatorCtx,
            exceptionAggregator => {
                var injectorMetadataMap = sourceMetadata.MetadataTypeMap.InjectorMetadataMap;
                var dependencyMetadataMap = sourceMetadata.MetadataTypeMap.DependencyMetadataMap;

                return sourceMetadata.MetadataTypeMap.InjectorMetadataMap.Values.SelectCatching(
                        exceptionAggregator,
                        injectorMetadata => $"extracting injector {injectorMetadata.InjectorInterfaceType}",
                        injectorMetadata => {
                            if (!sourceMetadata.MetadataTypeMap
                                .InjectorSpecMetadataListMap.TryGetValue(injectorMetadata.InjectorInterfaceType,
                                    out var injectorSpecMap)
                            ) {
                                throw Diagnostics.InternalError.AsException(
                                    $"Cannot find required injector metadata map for {injectorMetadata.InjectorInterfaceType}.",
                                    injectorMetadata.Location,
                                    generatorCtx);
                            }

                            var defGenerationCtx = new DefGenerationContext(generatorCtx);

                            return injectionContextDefMapper.Map(injectorMetadata,
                                injectorMetadataMap,
                                injectorSpecMap,
                                dependencyMetadataMap,
                                defGenerationCtx);
                        })
                    .ToImmutableList();
            });
    }
}
