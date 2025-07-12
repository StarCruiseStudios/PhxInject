// -----------------------------------------------------------------------------
//  <copyright file="SpecDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecDesc(
    TypeModel SpecType,
    SpecInstantiationMode InstantiationMode,
    IEnumerable<SpecFactoryDesc> Factories,
    IEnumerable<SpecBuilderDesc> Builders,
    IEnumerable<SpecLinkDesc> Links
) : IDescriptor {
    public Location Location {
        get => SpecType.Location;
    }

    public interface IExtractor {
        SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext extractorCtx);
    }

    public class Extractor : IExtractor {
        private readonly LinkAttributeMetadata.IExtractor linkAttributeExtractor;
        private readonly SpecBuilderDesc.IExtractor specExtractorDescExtractor;
        private readonly SpecFactoryDesc.IExtractor specFactoryDescExtractor;
        private readonly SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor;
        private readonly SpecLinkDesc.IExtractor specLinkDescExtractor;

        public Extractor(
            SpecFactoryDesc.IExtractor specFactoryDescExtractor,
            SpecBuilderDesc.IExtractor specExtractorDescExtractor,
            SpecLinkDesc.IExtractor specLinkDescExtractor,
            SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor,
            LinkAttributeMetadata.IExtractor linkAttributeExtractor
        ) {
            this.specFactoryDescExtractor = specFactoryDescExtractor;
            this.specExtractorDescExtractor = specExtractorDescExtractor;
            this.specLinkDescExtractor = specLinkDescExtractor;
            this.specificationAttributeExtractor = specificationAttributeExtractor;
            this.linkAttributeExtractor = linkAttributeExtractor;
        }

        public Extractor() : this(
            new SpecFactoryDesc.Extractor(),
            new SpecBuilderDesc.Extractor(),
            new SpecLinkDesc.Extractor(),
            SpecificationAttributeMetadata.Extractor.Instance,
            LinkAttributeMetadata.Extractor.Instance
        ) { }

        public SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext extractorCtx) {
            return extractorCtx.UseChildContext(
                "extracting specification",
                specSymbol,
                currentCtx => {
                    var specLocation = specSymbol.Locations.First();
                    var specType = TypeModel.FromTypeSymbol(specSymbol);

                    var specAttribute = specificationAttributeExtractor
                        .Extract(specSymbol, currentCtx);

                    var specInstantiationMode = specSymbol.IsStatic
                        ? SpecInstantiationMode.Static
                        : SpecInstantiationMode.Instantiated;

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

                    IReadOnlyList<SpecFactoryDesc> factories = specFields
                        .SelectCatching(
                            currentCtx.Aggregator,
                            field =>
                                $"extracting specification factory reference field {specType}.{field.Name}",
                            field => specFactoryDescExtractor.ExtractFactoryReference(field, currentCtx))
                        .Concat(specProperties
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop =>
                                    $"extracting specification factory reference property {specType}.{prop.Name}",
                                prop => specFactoryDescExtractor.ExtractFactoryReference(prop, currentCtx)))
                        .Concat(specProperties
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop => $"extracting specification factory property {specType}.{prop.Name}",
                                prop => specFactoryDescExtractor.ExtractFactory(prop, currentCtx)))
                        .Concat(specMethods
                            .SelectCatching(
                                currentCtx.Aggregator,
                                method => $"extracting specification factory method {specType}.{method.Name}",
                                method => specFactoryDescExtractor.ExtractFactory(method, currentCtx)))
                        .OfType<SpecFactoryDesc>()
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderDesc> builders = specFields
                        .SelectCatching(
                            currentCtx.Aggregator,
                            field =>
                                $"extracting specification builder reference field {specType}.{field.Name}",
                            field => specExtractorDescExtractor.ExtractBuilderReference(field, currentCtx))
                        .Concat(specProperties
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop =>
                                    $"extracting specification builder reference property {specType}.{prop.Name}",
                                prop => specExtractorDescExtractor.ExtractBuilderReference(prop, currentCtx)))
                        .Concat(specMethods
                            .SelectCatching(
                                currentCtx.Aggregator,
                                method => $"extracting specification builder method {specType}.{method.Name}",
                                method => specExtractorDescExtractor.ExtractBuilder(method, currentCtx)))
                        .OfType<SpecBuilderDesc>()
                        .ToImmutableList();

                    IReadOnlyList<SpecLinkDesc> links = linkAttributeExtractor.ExtractAll(specSymbol, currentCtx)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            link => $"extracting specification link from {specType}",
                            link => specLinkDescExtractor.Extract(
                                link,
                                specLocation,
                                currentCtx))
                        .ToImmutableList();

                    return new SpecDesc(
                        specType,
                        specInstantiationMode,
                        factories,
                        builders,
                        links);
                });
        }
    }

    public interface IAutoSpecExtractor {
        SpecDesc Extract(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes,
            ExtractorContext extractorCtx);
    }

    public class AutoSpecExtractor : IAutoSpecExtractor {
        private readonly LinkAttributeMetadata.IExtractor linkAttributeExtractor;
        private readonly SpecBuilderDesc.IExtractor specExtractorDescExtractor;
        private readonly SpecFactoryDesc.IExtractor specFactoryDescExtractor;
        private readonly SpecLinkDesc.IExtractor specLinkDescExtractor;

        public AutoSpecExtractor(
            SpecFactoryDesc.IExtractor specFactoryDescExtractor,
            SpecBuilderDesc.IExtractor specExtractorDescExtractor,
            SpecLinkDesc.IExtractor specLinkDescExtractor,
            LinkAttributeMetadata.IExtractor linkAttributeExtractor
        ) {
            this.specFactoryDescExtractor = specFactoryDescExtractor;
            this.specExtractorDescExtractor = specExtractorDescExtractor;
            this.specLinkDescExtractor = specLinkDescExtractor;
            this.linkAttributeExtractor = linkAttributeExtractor;
        }

        public AutoSpecExtractor() : this(
            new SpecFactoryDesc.Extractor(),
            new SpecBuilderDesc.Extractor(),
            new SpecLinkDesc.Extractor(),
            LinkAttributeMetadata.Extractor.Instance
        ) { }

        public SpecDesc Extract(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes,
            ExtractorContext extractorCtx
        ) {
            var specLocation = injectorType.TypeSymbol.Locations.First();
            var specType = MetadataHelpers.CreateConstructorSpecContainerType(injectorType);

            return extractorCtx.UseChildContext(
                "extracting auto constructor specification",
                injectorType.TypeSymbol,
                currentCtx => {
                    var specInstantiationMode = SpecInstantiationMode.Static;

                    IReadOnlyList<SpecFactoryDesc> autoConstructorFactories = constructorTypes
                        .SelectCatching(
                            currentCtx.Aggregator,
                            constructorType => $"extracting auto constructor factory for type {constructorType}",
                            constructorType =>
                                specFactoryDescExtractor.ExtractAutoConstructorFactory(constructorType, currentCtx))
                        .ToImmutableList();

                    IReadOnlyList<SpecLinkDesc> links = constructorTypes
                        .SelectMany(constructorType => linkAttributeExtractor
                            .ExtractAll(constructorType.TypeModel.TypeSymbol, currentCtx)
                            .SelectCatching(
                                currentCtx.Aggregator,
                                link => $"extracting link for auto constructor type {constructorType}",
                                link => specLinkDescExtractor
                                    .Extract(link, specLocation, currentCtx)
                                    .Also(it => {
                                        if (it.InputType != constructorType) {
                                            throw Diagnostics.InvalidSpecification.AsException(
                                                $"Auto constructed type {constructorType} must link to itself. Found link with input type {it.InputType}.",
                                                constructorType.TypeModel.TypeSymbol.Locations.First(),
                                                currentCtx);
                                        }
                                    })))
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderDesc> autoBuilders = builderTypes
                        .SelectCatching(
                            currentCtx.Aggregator,
                            builderType => $"extracting auto builder for type {builderType}",
                            builderType => specExtractorDescExtractor.ExtractAutoBuilder(builderType, currentCtx))
                        .ToImmutableList();

                    return new SpecDesc(
                        specType,
                        specInstantiationMode,
                        autoConstructorFactories,
                        autoBuilders,
                        links);
                });
        }
    }
}
