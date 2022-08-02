// -----------------------------------------------------------------------------
//  <copyright file="SpecDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;

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
            private readonly CreateSpecFactoryDescriptor createSpecFactoryDescriptor;
            private readonly CreateSpecBuilderDescriptor createSpecBuilderDescriptor;
            private readonly CreateSpecLinkDescriptor createSpecLinkDescriptor;

            public Builder(
                    CreateSpecFactoryDescriptor createSpecFactoryDescriptor,
                    CreateSpecBuilderDescriptor createSpecBuilderDescriptor,
                    CreateSpecLinkDescriptor createSpecLinkDescriptor
            ) {
                this.createSpecFactoryDescriptor = createSpecFactoryDescriptor;
                this.createSpecBuilderDescriptor = createSpecBuilderDescriptor;
                this.createSpecLinkDescriptor = createSpecLinkDescriptor;
            }

            public SpecDescriptor Build(ITypeSymbol specSymbol, DescriptorGenerationContext context) {
                var specType = TypeModel.FromTypeSymbol(specSymbol);
                var specLocation = specSymbol.Locations.First();
                var specInstantiationMode = specSymbol.IsStatic
                        ? SpecInstantiationMode.Static
                        : SpecInstantiationMode.Instantiated;

                var specMethods = specSymbol.GetMembers().OfType<IMethodSymbol>();

                var factories = specMethods.Select(method => createSpecFactoryDescriptor(method, context))
                        .Where(factory => factory != null)
                        .Select(factory => factory!)
                        .ToImmutableList();
                var builders = specMethods.Select(builder => createSpecBuilderDescriptor(builder, context))
                        .Where(builder => builder != null)
                        .Select(builder => builder!)
                        .ToImmutableList();

                var linkAttributes = SymbolProcessors.GetLinkAttributes(specSymbol);
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
