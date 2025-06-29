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

    internal record SpecDescriptor(
        TypeModel SpecType,
        SpecInstantiationMode InstantiationMode,
        IEnumerable<SpecFactoryDescriptor> Factories,
        IEnumerable<SpecBuilderDescriptor> Builders,
        IEnumerable<SpecLinkDescriptor> Links,
        Location Location
    ) : IDescriptor {
        public interface IBuilder {
            SpecDescriptor Build(ITypeSymbol specSymbol, DescriptorGenerationContext context);
            
            SpecDescriptor BuildConstructorSpec(
                TypeModel injectorType,
                IReadOnlyList<QualifiedTypeModel> constructorTypes,
                IReadOnlyList<QualifiedTypeModel> builderTypes);
        }
        public class Builder : IBuilder {
            private readonly SpecFactoryDescriptor.IBuilder SpecFactoryDescriptorBuilder;
            private readonly SpecBuilderDescriptor.IBuilder specBuilderDescriptorBuilder;
            private readonly SpecLinkDescriptor.IBuilder SpecLinkDescriptorBuilder;

            public Builder(
                SpecFactoryDescriptor.IBuilder specFactoryDescriptorBuilder,
                SpecBuilderDescriptor.IBuilder specBuilderDescriptorBuilder,
                SpecLinkDescriptor.IBuilder specLinkDescriptorBuilder
            ) {
                this.SpecFactoryDescriptorBuilder = specFactoryDescriptorBuilder;
                this.specBuilderDescriptorBuilder = specBuilderDescriptorBuilder;
                this.SpecLinkDescriptorBuilder = specLinkDescriptorBuilder;
            }

            public Builder() : this(
                new SpecFactoryDescriptor.Builder(),
                new SpecBuilderDescriptor.Builder(),
                new SpecLinkDescriptor.Builder()) { }

            public SpecDescriptor Build(ITypeSymbol specSymbol, DescriptorGenerationContext context) {
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
                    .Select(prop => SpecFactoryDescriptorBuilder.BuildFactoryReference(prop, context))
                    .Where(factoryReference => factoryReference != null)
                    .Select(factoryReference => factoryReference!);
                var factoryReferenceProperties = specProperties
                    .Select(prop => SpecFactoryDescriptorBuilder.BuildFactoryReference(prop, context))
                    .Where(factoryReference => factoryReference != null)
                    .Select(factoryReference => factoryReference!);
                var factoryProperties = specProperties
                    .Select(prop => SpecFactoryDescriptorBuilder.BuildFactory(prop, context))
                    .Where(factory => factory != null)
                    .Select(factory => factory!);
                var factoryMethods = specMethods.Select(method => SpecFactoryDescriptorBuilder.BuildFactory(method, context))
                    .Where(factory => factory != null)
                    .Select(factory => factory!);

                var factories = factoryMethods
                    .Concat(factoryProperties)
                    .Concat(factoryReferenceFields)
                    .Concat(factoryReferenceProperties)
                    .ToImmutableList();

                var builderReferenceFields = specFields
                    .Select(builderReference =>
                        specBuilderDescriptorBuilder.BuildBuilderReference(builderReference))
                    .Where(builderReference => builderReference != null)
                    .Select(builderReference => builderReference!);
                var builderReferenceProperties = specProperties
                    .Select(builderReference =>
                        specBuilderDescriptorBuilder.BuildBuilderReference(builderReference))
                    .Where(builderReference => builderReference != null)
                    .Select(builderReference => builderReference!);
                var builderMethods = specMethods.Select(builder => specBuilderDescriptorBuilder.BuildBuilder(builder))
                    .Where(builder => builder != null)
                    .Select(builder => builder!);

                var builders = builderMethods
                    .Concat(builderReferenceProperties)
                    .Concat(builderReferenceFields)
                    .ToImmutableList();

                var linkAttributes = specSymbol.GetLinkAttributes();
                var links = linkAttributes.Select(link => SpecLinkDescriptorBuilder.Build(link, specLocation, context));
                return new SpecDescriptor(
                    specType,
                    specInstantiationMode,
                    factories,
                    builders,
                    links,
                    specLocation);
            }
            
            public SpecDescriptor BuildConstructorSpec(
                TypeModel injectorType,
                IReadOnlyList<QualifiedTypeModel> constructorTypes,
                IReadOnlyList<QualifiedTypeModel> builderTypes
            ) {
                var specLocation = injectorType.typeSymbol.Locations.First();
                var specType = TypeHelpers.CreateConstructorSpecContainerType(injectorType);
                var specInstantiationMode = SpecInstantiationMode.Static;

                var factories = constructorTypes
                    .Select(type => SpecFactoryDescriptorBuilder.BuildConstructorFactory(type))
                    .ToImmutableList();

                var builders = builderTypes
                    .Select(type => specBuilderDescriptorBuilder.BuildDirectBuilder(type))
                    .ToImmutableList();

                return new SpecDescriptor(
                    specType,
                    specInstantiationMode,
                    factories,
                    builders,
                    ImmutableList<SpecLinkDescriptor>.Empty,
                    specLocation);
            }
        }
    }
}
