// -----------------------------------------------------------------------------
//  <copyright file="SpecDescriptor.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecDesc(
    TypeModel SpecType,
    SpecInstantiationMode InstantiationMode,
    IEnumerable<SpecFactoryDesc> Factories,
    IEnumerable<SpecBuilderDesc> Builders,
    IEnumerable<SpecLinkDesc> Links,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext extractorContext);
    }

    public class Extractor : IExtractor {
        private readonly SpecBuilderDesc.IExtractor specExtractorDescExtractor;
        private readonly SpecFactoryDesc.IExtractor specFactoryDescExtractor;
        private readonly SpecLinkDesc.IExtractor specLinkDescExtractor;

        public Extractor(
            SpecFactoryDesc.IExtractor specFactoryDescExtractor,
            SpecBuilderDesc.IExtractor specExtractorDescExtractor,
            SpecLinkDesc.IExtractor specLinkDescExtractor
        ) {
            this.specFactoryDescExtractor = specFactoryDescExtractor;
            this.specExtractorDescExtractor = specExtractorDescExtractor;
            this.specLinkDescExtractor = specLinkDescExtractor;
        }

        public Extractor() : this(
            new SpecFactoryDesc.Extractor(),
            new SpecBuilderDesc.Extractor(),
            new SpecLinkDesc.Extractor()) { }

        public SpecDesc Extract(ITypeSymbol specSymbol, ExtractorContext extractorContext) {
            var specLocation = specSymbol.Locations.First();
            var specType = TypeModel.FromTypeSymbol(specSymbol);

            return ExceptionAggregator.Try(
                $"extracting specification {specType}",
                specLocation,
                extractorContext.GenerationContext,
                exceptionAggregator => {
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
                            exceptionAggregator,
                            field => $"extracting specification factory reference field {specType}.{field.Name}",
                            field => specFactoryDescExtractor.ExtractFactoryReference(field, extractorContext))
                        .Concat(specProperties
                            .SelectCatching(
                                exceptionAggregator,
                                prop => $"extracting specification factory reference property {specType}.{prop.Name}",
                                prop => specFactoryDescExtractor.ExtractFactoryReference(prop, extractorContext)))
                        .Concat(specProperties
                            .SelectCatching(
                                exceptionAggregator,
                                prop => $"extracting specification factory property {specType}.{prop.Name}",
                                prop => specFactoryDescExtractor.ExtractFactory(prop, extractorContext)))
                        .Concat(specMethods
                            .SelectCatching(
                                exceptionAggregator,
                                method => $"extracting specification factory method {specType}.{method.Name}",
                                method => specFactoryDescExtractor.ExtractFactory(method, extractorContext)))
                        .OfType<SpecFactoryDesc>()
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderDesc> builders = specFields
                        .SelectCatching(
                            exceptionAggregator,
                            field => $"extracting specification builder reference field {specType}.{field.Name}",
                            field => specExtractorDescExtractor.ExtractBuilderReference(field, extractorContext))
                        .Concat(specProperties
                            .SelectCatching(
                                exceptionAggregator,
                                prop => $"extracting specification builder reference property {specType}.{prop.Name}",
                                prop => specExtractorDescExtractor.ExtractBuilderReference(prop, extractorContext)))
                        .Concat(specMethods
                            .SelectCatching(
                                exceptionAggregator,
                                method => $"extracting specification builder method {specType}.{method.Name}",
                                method => specExtractorDescExtractor.ExtractBuilder(method, extractorContext)))
                        .OfType<SpecBuilderDesc>()
                        .ToImmutableList();

                    IReadOnlyList<SpecLinkDesc> links = specSymbol.GetAllLinkAttributes()
                        .SelectCatching(
                            exceptionAggregator,
                            link => $"extracting specification link from {specType}",
                            link => specLinkDescExtractor.Extract(link, specLocation, extractorContext))
                        .ToImmutableList();

                    return new SpecDesc(
                        specType,
                        specInstantiationMode,
                        factories,
                        builders,
                        links,
                        specLocation);
                });
        }
    }

    public interface IAutoSpecExtractor {
        SpecDesc Extract(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes,
            ExtractorContext extractorContext);
    }

    public class AutoSpecExtractor : IAutoSpecExtractor {
        private readonly SpecBuilderDesc.IExtractor specExtractorDescExtractor;
        private readonly SpecFactoryDesc.IExtractor specFactoryDescExtractor;
        private readonly SpecLinkDesc.IExtractor specLinkDescExtractor;

        public AutoSpecExtractor(
            SpecFactoryDesc.IExtractor specFactoryDescExtractor,
            SpecBuilderDesc.IExtractor specExtractorDescExtractor,
            SpecLinkDesc.IExtractor specLinkDescExtractor
        ) {
            this.specFactoryDescExtractor = specFactoryDescExtractor;
            this.specExtractorDescExtractor = specExtractorDescExtractor;
            this.specLinkDescExtractor = specLinkDescExtractor;
        }

        public AutoSpecExtractor() : this(
            new SpecFactoryDesc.Extractor(),
            new SpecBuilderDesc.Extractor(),
            new SpecLinkDesc.Extractor()) { }

        public SpecDesc Extract(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes,
            ExtractorContext extractorContext
        ) {
            var specLocation = injectorType.typeSymbol.Locations.First();
            var specType = TypeHelpers.CreateConstructorSpecContainerType(injectorType);

            return ExceptionAggregator.Try(
                $"extracting auto specification for injector {injectorType}",
                specLocation,
                extractorContext.GenerationContext,
                exceptionAggregator => {
                    var specInstantiationMode = SpecInstantiationMode.Static;

                    IReadOnlyList<SpecFactoryDesc> autoConstructorFactories = constructorTypes
                        .SelectCatching(
                            exceptionAggregator,
                            constructorType => $"extracting auto constructor factory for type {constructorType}",
                            constructorType =>
                                specFactoryDescExtractor.ExtractAutoConstructorFactory(constructorType,
                                    extractorContext))
                        .ToImmutableList();

                    IReadOnlyList<SpecLinkDesc> links = constructorTypes
                        .SelectMany(constructorType => constructorType.TypeModel.typeSymbol.GetAllLinkAttributes()
                            .SelectCatching(
                                exceptionAggregator,
                                link => $"extracting link for auto constructor type {constructorType}",
                                link => specLinkDescExtractor
                                    .Extract(link, specLocation, extractorContext)
                                    .Also(it => {
                                        if (it.InputType != constructorType) {
                                            throw Diagnostics.InvalidSpecification.AsException(
                                                $"Auto constructed type {constructorType} must link to itself. Found link with input type {it.InputType}.",
                                                constructorType.TypeModel.typeSymbol.Locations.First(),
                                                extractorContext.GenerationContext);
                                        }
                                    })))
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderDesc> autoBuilders = builderTypes
                        .SelectCatching(
                            exceptionAggregator,
                            builderType => $"extracting auto builder for type {builderType}",
                            builderType => specExtractorDescExtractor.ExtractAutoBuilder(builderType, extractorContext))
                        .ToImmutableList();

                    return new SpecDesc(
                        specType,
                        specInstantiationMode,
                        autoConstructorFactories,
                        autoBuilders,
                        links,
                        specLocation);
                });
        }
    }
}
