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
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecDesc(
    TypeModel SpecType,
    SpecInstantiationMode InstantiationMode,
    IEnumerable<SpecFactoryDesc> Factories,
    IEnumerable<SpecBuilderMetadata> Builders,
    IEnumerable<SpecLinkMetadata> Links
) : IDescriptor {
    public Location Location {
        get => SpecType.Location;
    }

    public interface IExtractor {
        SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext extractorCtx);
        SpecDesc ExtractDependencySpec(
            ITypeSymbol dependencySymbol,
            IReadOnlyList<DependencyProviderMetadata> providers,
            ExtractorContext extractorCtx);
    }

    public class Extractor : IExtractor {
        private readonly SpecBuilderMetadata.IAutoBuilderExtractor autoBuilderExtractor;
        private readonly SpecBuilderMetadata.IBuilderExtractor specBuilderExtractor;
        private readonly SpecBuilderMetadata.IBuilderReferenceExtractor specBuilderReferenceExtractor;
        private readonly SpecFactoryDesc.IExtractor specFactoryDescExtractor;
        private readonly SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor;
        private readonly SpecLinkMetadata.IExtractor specLinkExtractor;

        public Extractor(
            SpecBuilderMetadata.IAutoBuilderExtractor autoBuilderExtractor,
            SpecBuilderMetadata.IBuilderExtractor specBuilderExtractor,
            SpecBuilderMetadata.IBuilderReferenceExtractor specBuilderReferenceExtractor,
            SpecFactoryDesc.IExtractor specFactoryDescExtractor,
            SpecLinkMetadata.IExtractor specLinkExtractor,
            SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor
        ) {
            this.autoBuilderExtractor = autoBuilderExtractor;
            this.specBuilderExtractor = specBuilderExtractor;
            this.specBuilderReferenceExtractor = specBuilderReferenceExtractor;
            this.specFactoryDescExtractor = specFactoryDescExtractor;
            this.specLinkExtractor = specLinkExtractor;
            this.specificationAttributeExtractor = specificationAttributeExtractor;
        }

        public Extractor() : this(
            SpecBuilderMetadata.AutoBuilderExtractor.Instance,
            SpecBuilderMetadata.BuilderExtractor.Instance,
            SpecBuilderMetadata.BuilderReferenceExtractor.Instance,
            new SpecFactoryDesc.Extractor(),
            SpecLinkMetadata.Extractor.Instance,
            SpecificationAttributeMetadata.Extractor.Instance
        ) { }

        public SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext extractorCtx) {
            return extractorCtx.UseChildContext(
                "extracting specification",
                specSymbol,
                currentCtx => {
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

                    IReadOnlyList<SpecBuilderMetadata> builders = specFields
                        .Where(field => specBuilderReferenceExtractor.CanExtract(field))
                        .SelectCatching(
                            currentCtx.Aggregator,
                            field =>
                                $"extracting specification builder reference field {specType}.{field.Name}",
                            field => specBuilderReferenceExtractor.ExtractBuilderReference(field, currentCtx))
                        .Concat(specProperties
                            .Where(prop => specBuilderReferenceExtractor.CanExtract(prop))
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop =>
                                    $"extracting specification builder reference property {specType}.{prop.Name}",
                                prop => specBuilderReferenceExtractor.ExtractBuilderReference(prop, currentCtx)))
                        .Concat(specMethods
                            .Where(method => specBuilderExtractor.CanExtract(method))
                            .SelectCatching(
                                currentCtx.Aggregator,
                                method => $"extracting specification builder method {specType}.{method.Name}",
                                method => specBuilderExtractor.ExtractBuilder(method, currentCtx)))
                        .ToImmutableList();

                    var links = specLinkExtractor.CanExtract(specType)
                        ? specLinkExtractor.ExtractAll(specType, currentCtx)
                        : ImmutableList<SpecLinkMetadata>.Empty;

                    return new SpecDesc(
                        specType,
                        specInstantiationMode,
                        factories,
                        builders,
                        links);
                });
        }

        // TODO: This was super hacky just to get it out of DependencyDesc.Extractor.
        public SpecDesc ExtractDependencySpec(
            ITypeSymbol dependencySymbol,
            IReadOnlyList<DependencyProviderMetadata> providers,
            ExtractorContext extractorCtx
        ) {
            IReadOnlyList<SpecFactoryDesc> specFactories = providers.Select(provider => new SpecFactoryDesc(
                    provider.ProvidedType,
                    provider.ProviderMethodName,
                    SpecFactoryMemberType.Method,
                    ImmutableList<QualifiedTypeModel>.Empty,
                    ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                    FactoryFabricationMode.Recurrent,
                    provider.IsPartial,
                    provider.Location))
                .ToImmutableList();

            return new SpecDesc(
                dependencySymbol.ToTypeModel(),
                SpecInstantiationMode.Instantiated,
                specFactories,
                ImmutableList<SpecBuilderMetadata>.Empty,
                ImmutableList<SpecLinkMetadata>.Empty);
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
        private readonly SpecBuilderMetadata.IAutoBuilderExtractor autoBuilderExtractor;
        private readonly SpecBuilderMetadata.IBuilderExtractor specBuilderExtractor;
        private readonly SpecBuilderMetadata.IBuilderReferenceExtractor specBuilderReferenceExtractor;
        private readonly SpecFactoryDesc.IExtractor specFactoryDescExtractor;
        private readonly SpecLinkMetadata.IExtractor specLinkExtractor;

        public AutoSpecExtractor(
            SpecBuilderMetadata.IAutoBuilderExtractor autoBuilderExtractor,
            SpecBuilderMetadata.IBuilderExtractor specBuilderExtractor,
            SpecBuilderMetadata.IBuilderReferenceExtractor specBuilderReferenceExtractor,
            SpecFactoryDesc.IExtractor specFactoryDescExtractor,
            SpecLinkMetadata.IExtractor specLinkExtractor
        ) {
            this.autoBuilderExtractor = autoBuilderExtractor;
            this.specBuilderExtractor = specBuilderExtractor;
            this.specBuilderReferenceExtractor = specBuilderReferenceExtractor;
            this.specFactoryDescExtractor = specFactoryDescExtractor;
            this.specLinkExtractor = specLinkExtractor;
        }

        public AutoSpecExtractor() : this(
            SpecBuilderMetadata.AutoBuilderExtractor.Instance,
            SpecBuilderMetadata.BuilderExtractor.Instance,
            SpecBuilderMetadata.BuilderReferenceExtractor.Instance,
            new SpecFactoryDesc.Extractor(),
            SpecLinkMetadata.Extractor.Instance
        ) { }

        public SpecDesc Extract(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes,
            ExtractorContext extractorCtx
        ) {
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

                    IReadOnlyList<SpecLinkMetadata> links = constructorTypes
                        .Select(constructorTypeSymbol => constructorTypeSymbol.TypeModel)
                        .Where(constructorType => specLinkExtractor.CanExtract(constructorType))
                        .SelectMany(constructorType => specLinkExtractor.ExtractAll(constructorType, currentCtx))
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderMetadata> autoBuilders = builderTypes
                        .SelectCatching(
                            currentCtx.Aggregator,
                            builderType => $"extracting auto builder for type {builderType}",
                            builderType => autoBuilderExtractor.ExtractBuilder(builderType, currentCtx))
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
