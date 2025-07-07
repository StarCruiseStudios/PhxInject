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
using Phx.Inject.Common.Util;

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
            var currentCtx = extractorContext.GetChildContext(specSymbol);

            return ExceptionAggregator.Try(
                $"extracting specification {specType}",
                currentCtx,
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
                            field => specFactoryDescExtractor.ExtractFactoryReference(field, currentCtx))
                        .Concat(specProperties
                            .SelectCatching(
                                exceptionAggregator,
                                prop => $"extracting specification factory reference property {specType}.{prop.Name}",
                                prop => specFactoryDescExtractor.ExtractFactoryReference(prop, currentCtx)))
                        .Concat(specProperties
                            .SelectCatching(
                                exceptionAggregator,
                                prop => $"extracting specification factory property {specType}.{prop.Name}",
                                prop => specFactoryDescExtractor.ExtractFactory(prop, currentCtx)))
                        .Concat(specMethods
                            .SelectCatching(
                                exceptionAggregator,
                                method => $"extracting specification factory method {specType}.{method.Name}",
                                method => specFactoryDescExtractor.ExtractFactory(method, currentCtx)))
                        .OfType<SpecFactoryDesc>()
                        .ToImmutableList();

                    IReadOnlyList<SpecBuilderDesc> builders = specFields
                        .SelectCatching(
                            exceptionAggregator,
                            field => $"extracting specification builder reference field {specType}.{field.Name}",
                            field => specExtractorDescExtractor.ExtractBuilderReference(field, currentCtx))
                        .Concat(specProperties
                            .SelectCatching(
                                exceptionAggregator,
                                prop => $"extracting specification builder reference property {specType}.{prop.Name}",
                                prop => specExtractorDescExtractor.ExtractBuilderReference(prop, currentCtx)))
                        .Concat(specMethods
                            .SelectCatching(
                                exceptionAggregator,
                                method => $"extracting specification builder method {specType}.{method.Name}",
                                method => specExtractorDescExtractor.ExtractBuilder(method, currentCtx)))
                        .OfType<SpecBuilderDesc>()
                        .ToImmutableList();

                    IReadOnlyList<SpecLinkDesc> links = specSymbol.GetAllLinkAttributes()
                        .SelectCatching(
                            exceptionAggregator,
                            link => $"extracting specification link from {specType}",
                            link => specLinkDescExtractor.Extract(link.GetOrThrow(currentCtx),
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
            ExtractorContext extractorCtx
        ) {
            var specLocation = injectorType.TypeSymbol.Locations.First();
            var specType = MetadataHelpers.CreateConstructorSpecContainerType(injectorType);
            var currentCtx = extractorCtx.GetChildContext(injectorType.TypeSymbol);

            return ExceptionAggregator.Try(
                $"extracting auto specification for injector {injectorType}",
                currentCtx,
                exceptionAggregator => {
                    var specInstantiationMode = SpecInstantiationMode.Static;

                    IReadOnlyList<SpecFactoryDesc> autoConstructorFactories = constructorTypes
                        .SelectCatching(
                            exceptionAggregator,
                            constructorType => $"extracting auto constructor factory for type {constructorType}",
                            constructorType =>
                                specFactoryDescExtractor.ExtractAutoConstructorFactory(constructorType, currentCtx))
                        .ToImmutableList();

                    IReadOnlyList<SpecLinkDesc> links = constructorTypes
                        .SelectMany(constructorType => constructorType.TypeModel.TypeSymbol.GetAllLinkAttributes()
                            .SelectCatching(
                                exceptionAggregator,
                                link => $"extracting link for auto constructor type {constructorType}",
                                link => specLinkDescExtractor
                                    .Extract(link.GetOrThrow(currentCtx), specLocation, currentCtx)
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
                            exceptionAggregator,
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
