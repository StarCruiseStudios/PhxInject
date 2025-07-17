// -----------------------------------------------------------------------------
//  <copyright file="SpecMetadata.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record SpecMetadata(
    TypeModel SpecType,
    SpecInstantiationMode InstantiationMode,
    IEnumerable<SpecFactoryMetadata> Factories,
    IEnumerable<SpecBuilderMetadata> Builders,
    IEnumerable<SpecLinkMetadata> Links,
    SpecificationAttributeMetadata? SpecAttribute
) : IMetadata {
    public Location Location {
        get => SpecType.Location;
    }

    public interface IExtractor {
        SpecMetadata Extract(ITypeSymbol specSymbol, ExtractorContext parentCtx);
    }

    public class Extractor(
        SpecBuilderMetadata.IBuilderExtractor specBuilderExtractor,
        SpecBuilderMetadata.IBuilderReferenceExtractor specBuilderReferenceExtractor,
        SpecFactoryMetadata.IFactoryExtractor specFactoryExtractor,
        SpecFactoryMetadata.IFactoryReferenceExtractor specFactoryReferenceExtractor,
        SpecLinkMetadata.IExtractor specLinkExtractor,
        SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            SpecBuilderMetadata.BuilderExtractor.Instance,
            SpecBuilderMetadata.BuilderReferenceExtractor.Instance,
            SpecFactoryMetadata.FactoryExtractor.Instance,
            SpecFactoryMetadata.FactoryReferenceExtractor.Instance,
            SpecLinkMetadata.Extractor.Instance,
            SpecificationAttributeMetadata.Extractor.Instance
        );

        public SpecMetadata Extract(ITypeSymbol specSymbol, ExtractorContext parentCtx) {
            return parentCtx.UseChildExtractorContext(
                $"extracting specification {specSymbol}",
                specSymbol,
                currentCtx => {
                    var specType = specSymbol.ToTypeModel();

                    var specAttribute = specificationAttributeExtractor
                        .Extract(specSymbol, currentCtx);

                    var specInstantiationMode = specSymbol switch {
                        { TypeKind: TypeKind.Interface } => SpecInstantiationMode.Instantiated,
                        { IsStatic: true } => SpecInstantiationMode.Static,
                        _ => SpecInstantiationMode.Instantiated
                    };

                    IReadOnlyList<ISymbol> specMembers = specSymbol.GetMembers().ToImmutableList();
                    IReadOnlyList<IFieldSymbol> specFields = specMembers
                        .OfType<IFieldSymbol>()
                        .ToImmutableList();
                    IReadOnlyList<IPropertySymbol> specProperties = specMembers
                        .OfType<IPropertySymbol>()
                        .ToImmutableList();
                    IReadOnlyList<IMethodSymbol> specMethods = specMembers
                        .OfType<IMethodSymbol>()
                        .ToImmutableList();

                    IReadOnlyList<SpecFactoryMetadata> factories = specFields
                        .Where(specFactoryReferenceExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            field =>
                                $"extracting specification factory reference field {specType}.{field.Name}",
                            field => specFactoryReferenceExtractor.ExtractFactoryReference(field, currentCtx))
                        .Concat(specProperties
                            .Where(specFactoryReferenceExtractor.CanExtract)
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop =>
                                    $"extracting specification factory reference property {specType}.{prop.Name}",
                                prop => specFactoryReferenceExtractor.ExtractFactoryReference(prop, currentCtx)))
                        .Concat(specProperties
                            .Where(specFactoryExtractor.CanExtract)
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop => $"extracting specification factory property {specType}.{prop.Name}",
                                prop => specFactoryExtractor.ExtractFactory(prop, currentCtx)))
                        .Concat(specMethods
                            .Where(specFactoryExtractor.CanExtract)
                            .SelectCatching(
                                currentCtx.Aggregator,
                                method => $"extracting specification factory method {specType}.{method.Name}",
                                method => specFactoryExtractor.ExtractFactory(method, currentCtx)))
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderMetadata> builders = specFields
                        .Where(specBuilderReferenceExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            field =>
                                $"extracting specification builder reference field {specType}.{field.Name}",
                            field => specBuilderReferenceExtractor.ExtractBuilderReference(field, currentCtx))
                        .Concat(specProperties
                            .Where(specBuilderReferenceExtractor.CanExtract)
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop =>
                                    $"extracting specification builder reference property {specType}.{prop.Name}",
                                prop => specBuilderReferenceExtractor.ExtractBuilderReference(prop, currentCtx)))
                        .Concat(specMethods
                            .Where(specBuilderExtractor.CanExtract)
                            .SelectCatching(
                                currentCtx.Aggregator,
                                method => $"extracting specification builder method {specType}.{method.Name}",
                                method => specBuilderExtractor.ExtractBuilder(method, currentCtx)))
                        .ToImmutableList();

                    var links = specLinkExtractor.CanExtract(specType)
                        ? specLinkExtractor.ExtractAll(specType, currentCtx)
                        : ImmutableList<SpecLinkMetadata>.Empty;

                    return new SpecMetadata(
                        specType,
                        specInstantiationMode,
                        factories,
                        builders,
                        links,
                        specAttribute);
                });
        }
    }

    public interface IAutoSpecExtractor {
        bool CanExtract(
            TypeModel injectorType,
            ISet<QualifiedTypeModel> neededFactoryTypes,
            ISet<QualifiedTypeModel> neededBuilderTypes);
        SpecMetadata Extract(
            TypeModel injectorType,
            ISet<QualifiedTypeModel> factoryTypes,
            ISet<QualifiedTypeModel> builderTypes,
            ExtractorContext parentCtx);
    }

    public class AutoSpecExtractor(
        SpecBuilderMetadata.IAutoBuilderExtractor autoBuilderExtractor,
        SpecFactoryMetadata.IAutoFactoryExtractor autoFactoryExtractor,
        SpecLinkMetadata.IExtractor specLinkExtractor
    ) : IAutoSpecExtractor {
        public static readonly IAutoSpecExtractor Instance = new AutoSpecExtractor(
            SpecBuilderMetadata.AutoBuilderExtractor.Instance,
            SpecFactoryMetadata.AutoFactoryExtractor.Instance,
            SpecLinkMetadata.Extractor.Instance
        );

        public bool CanExtract(
            TypeModel injectorType,
            ISet<QualifiedTypeModel> neededFactoryTypes,
            ISet<QualifiedTypeModel> neededBuilderTypes
        ) {
            return neededFactoryTypes.Any() || neededBuilderTypes.Any();
        }

        public SpecMetadata Extract(
            TypeModel injectorType,
            ISet<QualifiedTypeModel> factoryTypes,
            ISet<QualifiedTypeModel> builderTypes,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting auto constructor specification for injector {injectorType}",
                injectorType.TypeSymbol,
                currentCtx => {
                    var specContainerTypeName = NameHelpers.GetAppendedClassName(injectorType, "ConstructorFactories");
                    var specType = injectorType with {
                        BaseTypeName = specContainerTypeName,
                        TypeArguments = ImmutableList<TypeModel>.Empty
                    };
                    var specInstantiationMode = SpecInstantiationMode.Static;

                    IReadOnlyList<SpecFactoryMetadata> autoConstructorFactories = factoryTypes
                        .SelectCatching(
                            currentCtx.Aggregator,
                            constructorType => $"extracting auto constructor factory for type {constructorType}",
                            constructorType =>
                                autoFactoryExtractor.ExtractFactory(constructorType, currentCtx))
                        .ToImmutableList();

                    IReadOnlyList<SpecLinkMetadata> links = factoryTypes
                        .Select(constructorTypeSymbol => constructorTypeSymbol.TypeModel)
                        .Where(specLinkExtractor.CanExtract)
                        .SelectMany(constructorType => specLinkExtractor.ExtractAll(constructorType, currentCtx))
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderMetadata> autoBuilders = builderTypes
                        .SelectCatching(
                            currentCtx.Aggregator,
                            builderType => $"extracting auto builder for type {builderType}",
                            builderType => autoBuilderExtractor.ExtractBuilder(builderType, currentCtx))
                        .ToImmutableList();

                    return new SpecMetadata(
                        specType,
                        specInstantiationMode,
                        autoConstructorFactories,
                        autoBuilders,
                        links,
                        null);
                });
        }
    }
}
