// -----------------------------------------------------------------------------
//  <copyright file="InjectorDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;

    internal delegate InjectorDescriptor CreateInjectorDescriptor(
            ITypeSymbol injectorInterfaceSymbol,
            IDescriptorGenerationContext context
    );

    internal record InjectorDescriptor(
            TypeModel InjectorInterfaceType,
            string GeneratedInjectorTypeName,
            IEnumerable<TypeModel> SpecificationsTypes,
            IEnumerable<TypeModel> ExternalDependencyInterfaceTypes,
            IEnumerable<InjectorProviderDescriptor> Providers,
            IEnumerable<InjectorBuilderDescriptor> Builders,
            IEnumerable<InjectorChildFactoryDescriptor> ChildFactories,
            Location Location
    ) : IDescriptor {
        public class Builder {
            private readonly CreateInjectorProviderDescriptor createInjectorProvider;
            private readonly CreateInjectorBuilderDescriptor createInjectorBuilder;
            private readonly CreateInjectorChildFactoryDescriptor createInjectorChildFactory;

            public Builder(
                    CreateInjectorProviderDescriptor createInjectorProvider,
                    CreateInjectorBuilderDescriptor createInjectorBuilder,
                    CreateInjectorChildFactoryDescriptor createInjectorChildFactory
            ) {
                this.createInjectorProvider = createInjectorProvider;
                this.createInjectorBuilder = createInjectorBuilder;
                this.createInjectorChildFactory = createInjectorChildFactory;
            }

            public InjectorDescriptor Build(
                    ITypeSymbol injectorInterfaceSymbol,
                    IDescriptorGenerationContext context
            ) {
                var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
                var generatedInjectorTypeName = SymbolProcessors.GetGeneratedInjectorClassName(injectorInterfaceSymbol);
                var specificationTypes = SymbolProcessors.GetInjectorSpecificationTypes(injectorInterfaceSymbol)
                        .Select(specType => TypeModel.FromTypeSymbol(specType))
                        .ToImmutableList();

                var externalDependencyInterfaceTypes = SymbolProcessors
                        .GetExternalDependencyTypes(injectorInterfaceSymbol)
                        .Select(ediType => TypeModel.FromTypeSymbol(ediType))
                        .ToImmutableList();

                var injectorMethods = injectorInterfaceSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>();

                var providers = injectorMethods
                        .Select(method => createInjectorProvider(method, context))
                        .Where(provider => provider != null)
                        .Select(provider => provider!)
                        .ToImmutableList();

                var builders = injectorMethods
                        .Select(method => createInjectorBuilder(method, context))
                        .Where(builder => builder != null)
                        .Select(builder => builder!)
                        .ToImmutableList();

                var childFactories = injectorMethods
                        .Select(method => createInjectorChildFactory(method, context))
                        .Where(childFactory => childFactory != null)
                        .Select(childFactory => childFactory!)
                        .ToImmutableList();

                return new InjectorDescriptor(
                        injectorInterfaceType,
                        generatedInjectorTypeName,
                        specificationTypes,
                        externalDependencyInterfaceTypes,
                        providers,
                        builders,
                        childFactories,
                        injectorInterfaceSymbol.Locations.First());
            }
        }
    }
}
