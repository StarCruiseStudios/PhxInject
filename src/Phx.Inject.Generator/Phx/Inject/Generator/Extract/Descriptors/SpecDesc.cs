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
    IEnumerable<SpecFactoryMetadata> Factories,
    IEnumerable<SpecBuilderMetadata> Builders,
    IEnumerable<SpecLinkMetadata> Links
) : IDescriptor {
    public Location Location {
        get => SpecType.Location;
    }

    public interface IExtractor {
        SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext parentCtx);
        SpecDesc ExtractDependencySpec(
            ITypeSymbol dependencySymbol,
            IReadOnlyList<DependencyProviderMetadata> providers,
            ExtractorContext parentCtx);
    }

    public class Extractor : IExtractor {
        private readonly SpecBuilderMetadata.IBuilderExtractor specBuilderExtractor;
        private readonly SpecBuilderMetadata.IBuilderReferenceExtractor specBuilderReferenceExtractor;
        private readonly SpecFactoryMetadata.IFactoryExtractor specFactoryExtractor;
        private readonly SpecFactoryMetadata.IFactoryReferenceExtractor specFactoryReferenceExtractor;
        private readonly SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor;
        private readonly SpecLinkMetadata.IExtractor specLinkExtractor;

        public Extractor(
            SpecBuilderMetadata.IBuilderExtractor specBuilderExtractor,
            SpecBuilderMetadata.IBuilderReferenceExtractor specBuilderReferenceExtractor,
            SpecFactoryMetadata.IFactoryExtractor specFactoryExtractor,
            SpecFactoryMetadata.IFactoryReferenceExtractor specFactoryReferenceExtractor,
            SpecLinkMetadata.IExtractor specLinkExtractor,
            SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor
        ) {
            this.specBuilderExtractor = specBuilderExtractor;
            this.specBuilderReferenceExtractor = specBuilderReferenceExtractor;
            this.specFactoryExtractor = specFactoryExtractor;
            this.specFactoryReferenceExtractor = specFactoryReferenceExtractor;
            this.specLinkExtractor = specLinkExtractor;
            this.specificationAttributeExtractor = specificationAttributeExtractor;
        }

        public Extractor() : this(
            SpecBuilderMetadata.BuilderExtractor.Instance,
            SpecBuilderMetadata.BuilderReferenceExtractor.Instance,
            SpecFactoryMetadata.FactoryExtractor.Instance,
            SpecFactoryMetadata.FactoryReferenceExtractor.Instance,
            SpecLinkMetadata.Extractor.Instance,
            SpecificationAttributeMetadata.Extractor.Instance
        ) { }

        public SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext parentCtx) {
            return parentCtx.UseChildContext(
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

                    IReadOnlyList<SpecFactoryMetadata> factories = specFields
                        .Where(field => specFactoryReferenceExtractor.CanExtract(field))
                        .SelectCatching(
                            currentCtx.Aggregator,
                            field =>
                                $"extracting specification factory reference field {specType}.{field.Name}",
                            field => specFactoryReferenceExtractor.ExtractFactoryReference(field, currentCtx))
                        .Concat(specProperties
                            .Where(field => specFactoryReferenceExtractor.CanExtract(field))
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop =>
                                    $"extracting specification factory reference property {specType}.{prop.Name}",
                                prop => specFactoryReferenceExtractor.ExtractFactoryReference(prop, currentCtx)))
                        .Concat(specProperties
                            .Where(field => specFactoryExtractor.CanExtract(field))
                            .SelectCatching(
                                currentCtx.Aggregator,
                                prop => $"extracting specification factory property {specType}.{prop.Name}",
                                prop => specFactoryExtractor.ExtractFactory(prop, currentCtx)))
                        .Concat(specMethods
                            .Where(field => specFactoryExtractor.CanExtract(field))
                            .SelectCatching(
                                currentCtx.Aggregator,
                                method => $"extracting specification factory method {specType}.{method.Name}",
                                method => specFactoryExtractor.ExtractFactory(method, currentCtx)))
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
            ExtractorContext parentCtx
        ) {
            IReadOnlyList<SpecFactoryMetadata> specFactories = providers.Select(provider => new SpecFactoryMetadata(
                    provider.ProvidedType,
                    provider.ProviderMethodName,
                    SpecFactoryMemberType.Method,
                    ImmutableList<QualifiedTypeModel>.Empty,
                    ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                    FactoryFabricationMode.Recurrent,
                    provider.IsPartial,
                    provider.PartialAttribute,
                    null,
                    null,
                    provider.ProviderMethodSymbol))
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
            ExtractorContext parentCtx);
    }

    public class AutoSpecExtractor : IAutoSpecExtractor {
        private readonly SpecBuilderMetadata.IAutoBuilderExtractor autoBuilderExtractor;
        private readonly SpecFactoryMetadata.IAutoFactoryExtractor autoFactoryExtractor;
        private readonly SpecLinkMetadata.IExtractor specLinkExtractor;

        public AutoSpecExtractor(
            SpecBuilderMetadata.IAutoBuilderExtractor autoBuilderExtractor,
            SpecFactoryMetadata.IAutoFactoryExtractor autoFactoryExtractor,
            SpecLinkMetadata.IExtractor specLinkExtractor
        ) {
            this.autoBuilderExtractor = autoBuilderExtractor;
            this.autoFactoryExtractor = autoFactoryExtractor;
            this.specLinkExtractor = specLinkExtractor;
        }

        public AutoSpecExtractor() : this(
            SpecBuilderMetadata.AutoBuilderExtractor.Instance,
            SpecFactoryMetadata.AutoFactoryExtractor.Instance,
            SpecLinkMetadata.Extractor.Instance
        ) { }

        public SpecDesc Extract(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes,
            ExtractorContext parentCtx
        ) {
            var specType = MetadataHelpers.CreateConstructorSpecContainerType(injectorType);

            return parentCtx.UseChildContext(
                "extracting auto constructor specification",
                injectorType.TypeSymbol,
                currentCtx => {
                    var specInstantiationMode = SpecInstantiationMode.Static;

                    IReadOnlyList<SpecFactoryMetadata> autoConstructorFactories = constructorTypes
                        .SelectCatching(
                            currentCtx.Aggregator,
                            constructorType => $"extracting auto constructor factory for type {constructorType}",
                            constructorType =>
                                autoFactoryExtractor.ExtractFactory(constructorType, currentCtx))
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
