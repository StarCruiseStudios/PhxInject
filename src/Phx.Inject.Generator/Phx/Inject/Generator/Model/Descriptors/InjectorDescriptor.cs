// -----------------------------------------------------------------------------
//  <copyright file="InjectorDescriptor.cs" company="Star Cruise Studios LLC">
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

    internal delegate InjectorDescriptor CreateInjectorDescriptor(
            ITypeSymbol injectorInterfaceSymbol,
            IReadOnlyDictionary<TypeModel, SpecDescriptor> specDescriptors
    );

    internal record InjectorDescriptor(
            TypeModel InjectorType,
            TypeModel InjectorInterfaceType,
            IEnumerable<InjectorProviderDescriptor> Providers,
            IEnumerable<InjectorBuilderDescriptor> Builders,
            IEnumerable<SpecDescriptor> Specifications,
            Location Location
    ) : IDescriptor {
        public class Builder {
            private readonly CreateInjectorProviderDescriptor createInjectorProvider;
            private readonly CreateInjectorBuilderDescriptor createInjectorBuilder;

            public Builder(
                    CreateInjectorProviderDescriptor createInjectorProvider,
                    CreateInjectorBuilderDescriptor createInjectorBuilder
            ) {
                this.createInjectorProvider = createInjectorProvider;
                this.createInjectorBuilder = createInjectorBuilder;
            }

            public InjectorDescriptor Build(
                    ITypeSymbol injectorInterfaceSymbol,
                    IReadOnlyDictionary<TypeModel, SpecDescriptor> specDescriptors
            ) {
                var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
                var injectorClassName = SymbolProcessors.GetGeneratedInjectorClassName(injectorInterfaceSymbol);
                var injectorType = injectorInterfaceType with { TypeName = injectorClassName };

                var injectorMethods = injectorInterfaceSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>();

                var providerMethods = injectorMethods
                        .Select(method => createInjectorProvider(method))
                        .Where(provider => provider != null)
                        .Select(provider => provider!)
                        .ToImmutableList();

                var builderMethods = injectorMethods
                        .Select(method => createInjectorBuilder(method))
                        .Where(builder => builder != null)
                        .Select(builder => builder!)
                        .ToImmutableList();

                var specifications = SymbolProcessors.GetInjectorSpecificationTypes(injectorInterfaceSymbol)
                        .Select(
                                specType => {
                                    var specTypeModel = TypeModel.FromTypeSymbol(specType);
                                    if (specDescriptors.TryGetValue(specTypeModel, out var specDescriptor)) {
                                        return specDescriptor;
                                    }

                                    throw new InjectionException(
                                            Diagnostics.InvalidSpecification,
                                            $"Specification type {specTypeModel} required by injector {injectorInterfaceType} is not recognized as a valid specification.",
                                            specType.Locations.First());
                                });

                return new InjectorDescriptor(
                        injectorType,
                        injectorInterfaceType,
                        providerMethods,
                        builderMethods,
                        specifications,
                        injectorInterfaceSymbol.Locations.First());
            }
        }
    }
}
