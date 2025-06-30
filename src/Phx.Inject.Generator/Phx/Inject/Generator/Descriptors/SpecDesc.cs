// -----------------------------------------------------------------------------
//  <copyright file="SpecDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Descriptors {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Model;

    internal record SpecDesc(
        TypeModel SpecType,
        SpecInstantiationMode InstantiationMode,
        IEnumerable<SpecFactoryDesc> Factories,
        IEnumerable<SpecBuilderDesc> Builders,
        IEnumerable<SpecLinkDesc> Links,
        Location Location
    ) : IDescriptor {
        public interface IBuilder {
            SpecDesc Build(ITypeSymbol specSymbol, DescGenerationContext context);
            
            SpecDesc BuildConstructorSpec(
                TypeModel injectorType,
                IReadOnlyList<QualifiedTypeModel> constructorTypes,
                IReadOnlyList<QualifiedTypeModel> builderTypes);
        }
        public class Builder : IBuilder {
            private readonly SpecFactoryDesc.IBuilder SpecFactoryDescBuilder;
            private readonly SpecBuilderDesc.IBuilder specBuilderDescBuilder;
            private readonly SpecLinkDesc.IBuilder SpecLinkDescBuilder;

            public Builder(
                SpecFactoryDesc.IBuilder specFactoryDescBuilder,
                SpecBuilderDesc.IBuilder specBuilderDescBuilder,
                SpecLinkDesc.IBuilder specLinkDescBuilder
            ) {
                this.SpecFactoryDescBuilder = specFactoryDescBuilder;
                this.specBuilderDescBuilder = specBuilderDescBuilder;
                this.SpecLinkDescBuilder = specLinkDescBuilder;
            }

            public Builder() : this(
                new SpecFactoryDesc.Builder(),
                new SpecBuilderDesc.Builder(),
                new SpecLinkDesc.Builder()) { }

            public SpecDesc Build(ITypeSymbol specSymbol, DescGenerationContext context) {
                var specLocation = specSymbol.Locations.First();
                var specType = TypeModel.FromTypeSymbol(specSymbol);
                var specInstantiationMode = specSymbol.IsStatic
                    ? SpecInstantiationMode.Static
                    : SpecInstantiationMode.Instantiated;

                var specFields = specSymbol.GetMembers().OfType<IFieldSymbol>()
                    .ToImmutableArray();
                var specProperties = specSymbol.GetMembers().OfType<IPropertySymbol>()
                    .ToImmutableArray();
                var specMethods = specSymbol.GetMembers().OfType<IMethodSymbol>()
                    .ToImmutableArray();

                var factoryReferenceFields = specFields
                    .Select(prop => SpecFactoryDescBuilder.BuildFactoryReference(prop, context))
                    .Where(factoryReference => factoryReference != null)
                    .Select(factoryReference => factoryReference!);
                var factoryReferenceProperties = specProperties
                    .Select(prop => SpecFactoryDescBuilder.BuildFactoryReference(prop, context))
                    .Where(factoryReference => factoryReference != null)
                    .Select(factoryReference => factoryReference!);
                var factoryProperties = specProperties
                    .Select(prop => SpecFactoryDescBuilder.BuildFactory(prop, context))
                    .Where(factory => factory != null)
                    .Select(factory => factory!);
                var factoryMethods = specMethods.Select(method => SpecFactoryDescBuilder.BuildFactory(method, context))
                    .Where(factory => factory != null)
                    .Select(factory => factory!);

                var factories = factoryMethods
                    .Concat(factoryProperties)
                    .Concat(factoryReferenceFields)
                    .Concat(factoryReferenceProperties)
                    .ToImmutableList();

                var builderReferenceFields = specFields
                    .Select(builderReference =>
                        specBuilderDescBuilder.BuildBuilderReference(builderReference))
                    .Where(builderReference => builderReference != null)
                    .Select(builderReference => builderReference!);
                var builderReferenceProperties = specProperties
                    .Select(builderReference =>
                        specBuilderDescBuilder.BuildBuilderReference(builderReference))
                    .Where(builderReference => builderReference != null)
                    .Select(builderReference => builderReference!);
                var builderMethods = specMethods.Select(builder => specBuilderDescBuilder.BuildBuilder(builder))
                    .Where(builder => builder != null)
                    .Select(builder => builder!);

                var builders = builderMethods
                    .Concat(builderReferenceProperties)
                    .Concat(builderReferenceFields)
                    .ToImmutableList();

                var linkAttributes = specSymbol.GetLinkAttributes();
                var links = linkAttributes.Select(link => SpecLinkDescBuilder.Build(link, specLocation, context));
                return new SpecDesc(
                    specType,
                    specInstantiationMode,
                    factories,
                    builders,
                    links,
                    specLocation);
            }
            
            public SpecDesc BuildConstructorSpec(
                TypeModel injectorType,
                IReadOnlyList<QualifiedTypeModel> constructorTypes,
                IReadOnlyList<QualifiedTypeModel> builderTypes
            ) {
                var specLocation = injectorType.typeSymbol.Locations.First();
                var specType = TypeHelpers.CreateConstructorSpecContainerType(injectorType);
                var specInstantiationMode = SpecInstantiationMode.Static;

                var factories = constructorTypes
                    .Select(type => SpecFactoryDescBuilder.BuildConstructorFactory(type))
                    .ToImmutableList();

                var builders = builderTypes
                    .Select(type => specBuilderDescBuilder.BuildDirectBuilder(type))
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
}
