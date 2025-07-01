// -----------------------------------------------------------------------------
//  <copyright file="InjectorDescriptor.cs" company="Star Cruise Studios LLC">
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

internal record InjectorDesc(
    TypeModel InjectorInterfaceType,
    TypeModel InjectorType,
    IEnumerable<TypeModel> SpecificationsTypes,
    IEnumerable<TypeModel> DependencyInterfaceTypes,
    IEnumerable<InjectorProviderDesc> Providers,
    IEnumerable<ActivatorDesc> Builders,
    IEnumerable<InjectorChildFactoryDesc> ChildFactories,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        InjectorDesc Extract(
            ITypeSymbol injectorInterfaceSymbol,
            DescGenerationContext context
        );
    }

    public class Extractor : IExtractor {
        private readonly ActivatorDesc.IExtractor activatorDescExtractor;
        private readonly InjectorChildFactoryDesc.IExtractor injectorChildFactoryDescExtractor;
        private readonly InjectorProviderDesc.IExtractor injectorProviderDescriptionExtractor;

        public Extractor(
            InjectorProviderDesc.IExtractor injectorProviderDescriptionExtractor,
            ActivatorDesc.IExtractor activatorDescExtractor,
            InjectorChildFactoryDesc.IExtractor injectorChildFactoryDescExtractor
        ) {
            this.injectorProviderDescriptionExtractor = injectorProviderDescriptionExtractor;
            this.activatorDescExtractor = activatorDescExtractor;
            this.injectorChildFactoryDescExtractor = injectorChildFactoryDescExtractor;
        }

        public Extractor() : this(
            new InjectorProviderDesc.Extractor(),
            new ActivatorDesc.Extractor(),
            new InjectorChildFactoryDesc.Extractor()) { }

        public InjectorDesc Extract(
            ITypeSymbol injectorInterfaceSymbol,
            DescGenerationContext context
        ) {
            var injectorLocation = injectorInterfaceSymbol.Locations.First();
            var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
            var generatedInjectorTypeName = MetadataHelpers.GetGeneratedInjectorClassName(injectorInterfaceSymbol);
            var injectorType = injectorInterfaceType with {
                BaseTypeName = generatedInjectorTypeName,
                TypeArguments = ImmutableList<TypeModel>.Empty
            };

            return InjectionException.TryAggregate(aggregateException => {
                IReadOnlyList<TypeModel> dependencyInterfaceTypes = MetadataHelpers
                    .GetDependencyTypes(injectorInterfaceSymbol)
                    .Select(TypeModel.FromTypeSymbol)
                    .ToImmutableList();

                IReadOnlyList<TypeModel> specificationTypes = MetadataHelpers
                    .GetInjectorSpecificationTypes(injectorInterfaceSymbol)
                    .Select(TypeModel.FromTypeSymbol)
                    .Concat(dependencyInterfaceTypes)
                    .ToImmutableList();

                IReadOnlyList<IMethodSymbol> injectorMethods = injectorInterfaceSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .ToImmutableList();

                IReadOnlyList<InjectorProviderDesc> providers = aggregateException.Aggregate(() => injectorMethods
                        .SelectCatching(method => injectorProviderDescriptionExtractor.Extract(method, context))
                        .Where(provider => provider != null)
                        .Select(provider => provider!)
                        .ToImmutableList(),
                    ImmutableList.Create<InjectorProviderDesc>);

                IReadOnlyList<ActivatorDesc> builders = aggregateException.Aggregate(() => injectorMethods
                        .SelectCatching(method => activatorDescExtractor.Extract(method, context))
                        .Where(builder => builder != null)
                        .Select(builder => builder!)
                        .ToImmutableList(),
                    ImmutableList.Create<ActivatorDesc>);

                IReadOnlyList<InjectorChildFactoryDesc> childFactories = aggregateException.Aggregate(() =>
                        injectorMethods
                            .SelectCatching(method => injectorChildFactoryDescExtractor.Extract(method, context))
                            .Where(childFactory => childFactory != null)
                            .Select(childFactory => childFactory!)
                            .ToImmutableList(),
                    ImmutableList.Create<InjectorChildFactoryDesc>);

                return new InjectorDesc(
                    injectorInterfaceType,
                    injectorType,
                    specificationTypes,
                    dependencyInterfaceTypes,
                    providers,
                    builders,
                    childFactories,
                    injectorLocation);
            });
        }
    }
}
