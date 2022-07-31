// -----------------------------------------------------------------------------
//  <copyright file="SpecDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;

    internal delegate SpecDescriptor CreateSpecDescriptor(ITypeSymbol specSymbol);

    internal record SpecDescriptor(
            TypeModel SpecType,
            SpecInstantiationMode InstantiationMode,
            IEnumerable<SpecFactoryDescriptor> Factories,
            IEnumerable<SpecBuilderDescriptor> Builders,
            IEnumerable<LinkDescriptor> Links,
            Location Location
    ) : IDescriptor {
        public class Builder {
            private readonly CreateSpecFactoryDescriptor createSpecFactoryDescriptor;
            private readonly CreateSpecBuilderDescriptor createSpecBuilderDescriptor;
            private readonly CreateLinkDescriptor createLinkDescriptor;

            public Builder(
                    CreateSpecFactoryDescriptor createSpecFactoryDescriptor,
                    CreateSpecBuilderDescriptor createSpecBuilderDescriptor,
                    CreateLinkDescriptor createLinkDescriptor
            ) {
                this.createSpecFactoryDescriptor = createSpecFactoryDescriptor;
                this.createSpecBuilderDescriptor = createSpecBuilderDescriptor;
                this.createLinkDescriptor = createLinkDescriptor;
            }

            public SpecDescriptor Build(ITypeSymbol specSymbol) {
                var specType = TypeModel.FromTypeSymbol(specSymbol);
                var specLocation = specSymbol.Locations.First();
                var specInstantiationMode = specSymbol.IsStatic
                        ? SpecInstantiationMode.Static
                        : SpecInstantiationMode.Instantiated;

                var specMethods = specSymbol.GetMembers().OfType<IMethodSymbol>();

                var factories = specMethods.Select(method => createSpecFactoryDescriptor(method))
                        .Where(factory => factory != null)
                        .Select(factory => factory!)
                        .ToImmutableList();
                var builders = specMethods.Select(builder => createSpecBuilderDescriptor(builder))
                        .Where(builder => builder != null)
                        .Select(builder => builder!)
                        .ToImmutableList();

                var linkAttributes = SymbolProcessors.GetLinkAttributes(specSymbol);
                var links = linkAttributes.Select(link => createLinkDescriptor(link, specLocation));
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
