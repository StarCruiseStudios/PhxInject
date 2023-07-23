// -----------------------------------------------------------------------------
//  <copyright file="SpecDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;

    internal delegate SpecDescriptor CreateSpecDescriptor(ITypeSymbol specSymbol, DescriptorGenerationContext context);

    internal record SpecDescriptor(
            TypeModel SpecType,
            SpecInstantiationMode InstantiationMode,
            IEnumerable<SpecFactoryDescriptor> Factories,
            IEnumerable<SpecBuilderDescriptor> Builders,
            IEnumerable<SpecLinkDescriptor> Links,
            Location Location
    ) : IDescriptor {
        public class Builder {
            private readonly CreateSpecBuilderDescriptor createSpecBuilderDescriptor;
            private readonly CreateSpecBuilderReferencePropertyDescriptor createSpecBuilderReferencePropertyDescriptor;
            private readonly CreateSpecBuilderReferenceFieldDescriptor createSpecBuilderReferenceFieldDescriptor;
            private readonly CreateSpecFactoryMethodDescriptor createSpecFactoryMethodDescriptor;
            private readonly CreateSpecFactoryPropertyDescriptor createSpecFactoryPropertyDescriptor;
            private readonly CreateSpecFactoryReferencePropertyDescriptor createSpecFactoryReferencePropertyDescriptor;
            private readonly CreateSpecFactoryReferenceFieldDescriptor createSpecFactoryReferenceFieldDescriptor;
            private readonly CreateSpecLinkDescriptor createSpecLinkDescriptor;

            public Builder(
                    CreateSpecFactoryMethodDescriptor createSpecFactoryMethodDescriptor,
                    CreateSpecFactoryPropertyDescriptor createSpecFactoryPropertyDescriptor,
                    CreateSpecFactoryReferencePropertyDescriptor createSpecFactoryReferencePropertyDescriptor,
                    CreateSpecFactoryReferenceFieldDescriptor createSpecFactoryReferenceFieldDescriptor,
                    CreateSpecBuilderDescriptor createSpecBuilderDescriptor,
                    CreateSpecBuilderReferencePropertyDescriptor createSpecBuilderReferencePropertyDescriptor,
                    CreateSpecBuilderReferenceFieldDescriptor createSpecBuilderReferenceFieldDescriptor,
                    CreateSpecLinkDescriptor createSpecLinkDescriptor
            ) {
                this.createSpecFactoryMethodDescriptor = createSpecFactoryMethodDescriptor;
                this.createSpecFactoryPropertyDescriptor = createSpecFactoryPropertyDescriptor;
                this.createSpecFactoryReferencePropertyDescriptor = createSpecFactoryReferencePropertyDescriptor;
                this.createSpecFactoryReferenceFieldDescriptor = createSpecFactoryReferenceFieldDescriptor;
                this.createSpecBuilderDescriptor = createSpecBuilderDescriptor;
                this.createSpecBuilderReferencePropertyDescriptor = createSpecBuilderReferencePropertyDescriptor;
                this.createSpecBuilderReferenceFieldDescriptor = createSpecBuilderReferenceFieldDescriptor;
                this.createSpecLinkDescriptor = createSpecLinkDescriptor;
            }

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
                        .Select(prop => createSpecFactoryReferenceFieldDescriptor(prop, context))
                        .Where(factoryReference => factoryReference != null)
                        .Select(factoryReference => factoryReference!);
                var factoryReferenceProperties = specProperties
                        .Select(prop => createSpecFactoryReferencePropertyDescriptor(prop, context))
                        .Where(factoryReference => factoryReference != null)
                        .Select(factoryReference => factoryReference!);
                var factoryProperties = specProperties
                        .Select(prop => createSpecFactoryPropertyDescriptor(prop, context))
                        .Where(factory => factory != null)
                        .Select(factory => factory!);
                var factoryMethods = specMethods.Select(method => createSpecFactoryMethodDescriptor(method, context))
                        .Where(factory => factory != null)
                        .Select(factory => factory!);

                var factories = factoryMethods
                        .Concat(factoryProperties)
                        .Concat(factoryReferenceFields)
                        .Concat(factoryReferenceProperties)
                        .ToImmutableList();

                var builderReferenceFields = specFields
                        .Select(builderReference =>
                                createSpecBuilderReferenceFieldDescriptor(builderReference, context))
                        .Where(builderReference => builderReference != null)
                        .Select(builderReference => builderReference!);
                var builderReferenceProperties = specProperties
                        .Select(builderReference =>
                                createSpecBuilderReferencePropertyDescriptor(builderReference, context))
                        .Where(builderReference => builderReference != null)
                        .Select(builderReference => builderReference!);
                var builderMethods = specMethods.Select(builder => createSpecBuilderDescriptor(builder, context))
                        .Where(builder => builder != null)
                        .Select(builder => builder!);

                var builders = builderMethods
                        .Concat(builderReferenceProperties)
                        .Concat(builderReferenceFields)
                        .ToImmutableList();

                var linkAttributes = specSymbol.GetLinkAttributes();
                var links = linkAttributes.Select(link => createSpecLinkDescriptor(link, specLocation, context));
                return new SpecDescriptor(
                        specType,
                        specInstantiationMode,
                        factories,
                        builders,
                        links,
                        specLocation);
            }
        }
    }
}
