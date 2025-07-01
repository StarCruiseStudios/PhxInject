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
        SpecDesc Extract(ITypeSymbol specSymbol, DescGenerationContext context);

        SpecDesc ExtractConstructorSpec(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes);
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

        public SpecDesc Extract(ITypeSymbol specSymbol, DescGenerationContext context) {
            var specLocation = specSymbol.Locations.First();
            var specType = TypeModel.FromTypeSymbol(specSymbol);
            var specInstantiationMode = specSymbol.IsStatic
                ? SpecInstantiationMode.Static
                : SpecInstantiationMode.Instantiated;

            return InjectionException.TryAggregate(aggregateException => {
                IReadOnlyList<IFieldSymbol> specFields = specSymbol.GetMembers()
                    .OfType<IFieldSymbol>()
                    .ToImmutableArray();
                IReadOnlyList<IPropertySymbol> specProperties = specSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .ToImmutableArray();
                IReadOnlyList<IMethodSymbol> specMethods = specSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .ToImmutableArray();

                var factoryReferenceFields = aggregateException.Aggregate(() => specFields
                        .SelectCatching(prop => specFactoryDescExtractor.ExtractFactoryReference(prop, context))
                        .OfType<SpecFactoryDesc>(),
                    ImmutableList.Create<SpecFactoryDesc>);
                var factoryReferenceProperties = aggregateException.Aggregate(() => specProperties
                        .SelectCatching(prop => specFactoryDescExtractor.ExtractFactoryReference(prop, context))
                        .OfType<SpecFactoryDesc>(),
                    ImmutableList.Create<SpecFactoryDesc>);
                var factoryProperties = aggregateException.Aggregate(() => specProperties
                        .SelectCatching(prop => specFactoryDescExtractor.ExtractFactory(prop, context))
                        .OfType<SpecFactoryDesc>(),
                    ImmutableList.Create<SpecFactoryDesc>);
                var factoryMethods = aggregateException.Aggregate(() => specMethods
                        .SelectCatching(method => specFactoryDescExtractor.ExtractFactory(method, context))
                        .OfType<SpecFactoryDesc>(),
                    ImmutableList.Create<SpecFactoryDesc>);

                IReadOnlyList<SpecFactoryDesc> factories = factoryMethods
                    .Concat(factoryProperties)
                    .Concat(factoryReferenceFields)
                    .Concat(factoryReferenceProperties)
                    .ToImmutableList();

                var builderReferenceFields = aggregateException.Aggregate(() => specFields
                        .SelectCatching(builderReference =>
                            specExtractorDescExtractor.ExtractBuilderReference(builderReference))
                        .OfType<SpecBuilderDesc>(),
                    ImmutableList.Create<SpecBuilderDesc>);
                var builderReferenceProperties = aggregateException.Aggregate(() => specProperties
                        .SelectCatching(builderReference =>
                            specExtractorDescExtractor.ExtractBuilderReference(builderReference))
                        .OfType<SpecBuilderDesc>(),
                    ImmutableList.Create<SpecBuilderDesc>);
                var builderMethods = aggregateException.Aggregate(() => specMethods
                        .SelectCatching(builder => specExtractorDescExtractor.ExtractBuilder(builder))
                        .OfType<SpecBuilderDesc>(),
                    ImmutableList.Create<SpecBuilderDesc>);

                IReadOnlyList<SpecBuilderDesc> builders = builderMethods
                    .Concat(builderReferenceProperties)
                    .Concat(builderReferenceFields)
                    .ToImmutableList();

                var linkAttributes = specSymbol.GetLinkAttributes();
                IReadOnlyList<SpecLinkDesc> links = aggregateException.Aggregate(() => linkAttributes
                        .SelectCatching(link => specLinkDescExtractor.Extract(link, specLocation))
                        .ToImmutableList(),
                    ImmutableList.Create<SpecLinkDesc>);
                return new SpecDesc(
                    specType,
                    specInstantiationMode,
                    factories,
                    builders,
                    links,
                    specLocation);
            });
        }

        public SpecDesc ExtractConstructorSpec(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes
        ) {
            var specLocation = injectorType.typeSymbol.Locations.First();
            var specType = TypeHelpers.CreateConstructorSpecContainerType(injectorType);
            var specInstantiationMode = SpecInstantiationMode.Static;

            return InjectionException.TryAggregate(aggregateException => {
                IReadOnlyList<SpecFactoryDesc> factories = aggregateException.Aggregate(() => constructorTypes
                    .SelectCatching(type => specFactoryDescExtractor.ExtractConstructorFactory(type))
                    .ToImmutableList(),
                    ImmutableList.Create<SpecFactoryDesc>);

                IReadOnlyList<SpecBuilderDesc> builders = aggregateException.Aggregate(() => builderTypes
                    .SelectCatching(type => specExtractorDescExtractor.ExtractDirectBuilder(type))
                    .ToImmutableList(),
                    ImmutableList.Create<SpecBuilderDesc>);
                
                var links = aggregateException.Aggregate(() => constructorTypes
                    .SelectCatching(constructorType => {
                        return constructorType.TypeModel.typeSymbol.GetLinkAttributes()
                            .SelectCatching(link => {
                                var linkDesc = specLinkDescExtractor.Extract(link, specLocation);
                                if (linkDesc.InputType != constructorType) {
                                    throw new InjectionException(
                                        Diagnostics.InvalidSpecification,
                                        $"Auto constructed type {constructorType} can only link from itself. Found link with input type {linkDesc.InputType}.",
                                        constructorType.TypeModel.typeSymbol.Locations.First());
                                }
                                return linkDesc;
                            });
                    })
                    .SelectMany(flatten => flatten)
                    .ToImmutableList(),
                    ImmutableList.Create<SpecLinkDesc>);
                
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
}
