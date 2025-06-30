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

            IReadOnlyList<IFieldSymbol> specFields = specSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .ToImmutableArray();
            IReadOnlyList<IPropertySymbol> specProperties = specSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .ToImmutableArray();
            IReadOnlyList<IMethodSymbol> specMethods = specSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .ToImmutableArray();

            var factoryReferenceFields = specFields
                .Select(prop => specFactoryDescExtractor.ExtractFactoryReference(prop, context))
                .Where(factoryReference => factoryReference != null)
                .Select(factoryReference => factoryReference!);
            var factoryReferenceProperties = specProperties
                .Select(prop => specFactoryDescExtractor.ExtractFactoryReference(prop, context))
                .Where(factoryReference => factoryReference != null)
                .Select(factoryReference => factoryReference!);
            var factoryProperties = specProperties
                .Select(prop => specFactoryDescExtractor.ExtractFactory(prop, context))
                .Where(factory => factory != null)
                .Select(factory => factory!);
            var factoryMethods = specMethods
                .Select(method => specFactoryDescExtractor.ExtractFactory(method, context))
                .Where(factory => factory != null)
                .Select(factory => factory!);

            IReadOnlyList<SpecFactoryDesc> factories = factoryMethods
                .Concat(factoryProperties)
                .Concat(factoryReferenceFields)
                .Concat(factoryReferenceProperties)
                .ToImmutableList();

            var builderReferenceFields = specFields
                .Select(builderReference =>
                    specExtractorDescExtractor.ExtractBuilderReference(builderReference))
                .Where(builderReference => builderReference != null)
                .Select(builderReference => builderReference!);
            var builderReferenceProperties = specProperties
                .Select(builderReference =>
                    specExtractorDescExtractor.ExtractBuilderReference(builderReference))
                .Where(builderReference => builderReference != null)
                .Select(builderReference => builderReference!);
            var builderMethods = specMethods
                .Select(builder => specExtractorDescExtractor.ExtractBuilder(builder))
                .Where(builder => builder != null)
                .Select(builder => builder!);

            IReadOnlyList<SpecBuilderDesc> builders = builderMethods
                .Concat(builderReferenceProperties)
                .Concat(builderReferenceFields)
                .ToImmutableList();

            var linkAttributes = specSymbol.GetLinkAttributes();
            IReadOnlyList<SpecLinkDesc> links =
                linkAttributes.Select(link => specLinkDescExtractor.Extract(link, specLocation, context))
                    .ToImmutableList();
            return new SpecDesc(
                specType,
                specInstantiationMode,
                factories,
                builders,
                links,
                specLocation);
        }

        public SpecDesc ExtractConstructorSpec(
            TypeModel injectorType,
            IReadOnlyList<QualifiedTypeModel> constructorTypes,
            IReadOnlyList<QualifiedTypeModel> builderTypes
        ) {
            var specLocation = injectorType.typeSymbol.Locations.First();
            var specType = TypeHelpers.CreateConstructorSpecContainerType(injectorType);
            var specInstantiationMode = SpecInstantiationMode.Static;

            IReadOnlyList<SpecFactoryDesc> factories = constructorTypes
                .Select(type => specFactoryDescExtractor.ExtractConstructorFactory(type))
                .ToImmutableList();

            IReadOnlyList<SpecBuilderDesc> builders = builderTypes
                .Select(type => specExtractorDescExtractor.ExtractDirectBuilder(type))
                .ToImmutableList();

            return new SpecDesc(
                specType,
                specInstantiationMode,
                factories,
                builders,
                ImmutableList<SpecLinkDesc>.Empty,
                specLocation);
        }
    }
}
