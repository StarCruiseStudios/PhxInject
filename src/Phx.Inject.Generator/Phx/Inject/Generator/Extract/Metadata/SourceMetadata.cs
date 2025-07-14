// -----------------------------------------------------------------------------
// <copyright file="SourceMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata.Attributes;
using Phx.Inject.Generator.Extract.Metadata.Map;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record SourceMetadata(
    IReadOnlyList<InjectorMetadata> InjectorMetadata,
    IReadOnlyList<SpecMetadata> SpecMetadata,
    IReadOnlyList<DependencyMetadata> DependencyMetadata,
    MetadataTypeMap MetadataTypeMap
) {
    public interface IExtractor {
        SourceMetadata Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IGeneratorContext parentCtx);
    }

    public class Extractor(
        InjectorMetadata.IExtractor injectorExtractor,
        DependencyMetadata.IExtractor dependencyExtractor,
        SpecMetadata.IExtractor specExtractor,
        SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor,
        MetadataTypeMap.IExtractor metadataMapExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            Metadata.InjectorMetadata.Extractor.Instance,
            Metadata.DependencyMetadata.Extractor.Instance,
            Metadata.SpecMetadata.Extractor.Instance,
            SpecificationAttributeMetadata.Extractor.Instance,
            MetadataTypeMap.Extractor.Instance
        );

        public SourceMetadata Extract(
            IReadOnlyList<ITypeSymbol> injectorCandidates,
            IReadOnlyList<ITypeSymbol> specificationCandidates,
            IGeneratorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting injection source for assembly {parentCtx.ExecutionContext.Compilation.Assembly}",
                parentCtx.ExecutionContext.Compilation.Assembly,
                currentCtx => {
                    IReadOnlyList<InjectorMetadata> injectorMetadataList = injectorCandidates
                        .Where(injectorExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            injectorTypeSymbol => $"extracting injector from {injectorTypeSymbol}",
                            injectorTypeSymbol => injectorExtractor.Extract(injectorTypeSymbol, currentCtx))
                        .ToImmutableList();
                    currentCtx.Log($"Discovered {injectorMetadataList.Count} injector types.");

                    IReadOnlyList<DependencyMetadata> dependencyMetadataList = injectorMetadataList
                        .Where(injectorMetadata => injectorMetadata.DependencyInterfaceType != null)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            injectorMetadata =>
                                $"extracting dependencies from injector {injectorMetadata.InjectorInterfaceType}",
                            injectorMetadata => {
                                var dependencySymbol = injectorMetadata.DependencyInterfaceType!.TypeSymbol;
                                var injectorType = injectorMetadata.InjectorInterfaceType;
                                return dependencyExtractor.Extract(dependencySymbol, injectorType, currentCtx);
                            })
                        .ToImmutableList();
                    currentCtx.Log($"Discovered {dependencyMetadataList.Count} dependency types.");

                    IReadOnlyList<SpecMetadata> specMetadataList = specificationCandidates
                        .Where(specificationAttributeExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            specificationTypeSymbol => $"extracting specification from {specificationTypeSymbol}",
                            specificationTypeSymbol => specExtractor.Extract(specificationTypeSymbol, currentCtx))
                        .ToImmutableList();
                    currentCtx.Log($"Discovered {specMetadataList.Count} specification types.");

                    var metadataMap = metadataMapExtractor
                        .Extract(injectorMetadataList,
                            specMetadataList,
                            dependencyMetadataList,
                            currentCtx);
                    try {
                        var numTotalInjectorSpecifications =
                            metadataMap.InjectorSpecMetadataListMap.Values.Sum(it => it.Count);
                        var uniqueInjectorSpecifications = metadataMap.InjectorSpecMetadataListMap.Values.Aggregate(
                            new HashSet<TypeModel>(),
                            (acc, set) => {
                                acc.UnionWith(set.Keys);
                                return acc;
                            });
                        var unusedSpecifications = metadataMap.SpecMetadataMap.Keys
                            .Union(metadataMap.AutoFactorySpecMetadataMap.Keys)
                            .Except(uniqueInjectorSpecifications)
                            .ToImmutableHashSet();
                        currentCtx.Log($"Mapped {metadataMap.InjectorMetadataMap.Count} injectors.");
                        currentCtx.Log($"Mapped {metadataMap.DependencyMetadataMap.Count} injector dependencies.");
                        currentCtx.Log(
                            $"Mapped {numTotalInjectorSpecifications} injector specification types, {uniqueInjectorSpecifications.Count} unique.");
                        currentCtx.Log(
                            $"Mapped {metadataMap.SpecMetadataMap.Count} explicit specifications and {metadataMap.AutoFactorySpecMetadataMap.Count} auto factory specifications.");
                        currentCtx.Log($"Found {unusedSpecifications.Count} unused specifications.");
                        foreach (var unusedSpecification in unusedSpecifications) {
                            Diagnostics.UnusedSpecification.AsWarning(
                                $"Specification {unusedSpecification} is not used by any injector.",
                                unusedSpecification.Location,
                                currentCtx);
                        }
                    } catch (Exception e) {
                        currentCtx.Log(e.ToString());
                    }

                    return new SourceMetadata(
                        injectorMetadataList,
                        specMetadataList,
                        dependencyMetadataList,
                        metadataMap
                    );
                });
        }
    }
}
