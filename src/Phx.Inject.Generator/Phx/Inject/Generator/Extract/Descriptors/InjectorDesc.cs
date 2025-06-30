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
    string GeneratedInjectorTypeName,
    IEnumerable<TypeModel> SpecificationsTypes,
    IEnumerable<TypeModel> DependencyInterfaceTypes,
    IEnumerable<InjectorProviderDesc> Providers,
    IEnumerable<ActivatorDesc> Builders,
    IEnumerable<InjectorChildFactoryDesc> ChildFactories,
    Location Location
) : IDescriptor {
    public TypeModel InjectorType { get; } = InjectorInterfaceType with {
        BaseTypeName = GeneratedInjectorTypeName,
        TypeArguments = ImmutableList<TypeModel>.Empty
    };

    public interface IExtractor {
        InjectorDesc Extract(
            ITypeSymbol injectorInterfaceSymbol,
            DescGenerationContext context
        );
    }

    public class Extractor : IExtractor {
        private readonly ActivatorDesc.IExtractor injectorExtractorDescExtractor;
        private readonly InjectorChildFactoryDesc.IExtractor injectorChildFactoryDescExtractor;
        private readonly InjectorProviderDesc.IExtractor injectorProviderDescriptionExtractor;

        public Extractor(
            InjectorProviderDesc.IExtractor injectorProviderDescriptionExtractor,
            ActivatorDesc.IExtractor injectorExtractorDescExtractor,
            InjectorChildFactoryDesc.IExtractor injectorChildFactoryDescExtractor
        ) {
            this.injectorProviderDescriptionExtractor = injectorProviderDescriptionExtractor;
            this.injectorExtractorDescExtractor = injectorExtractorDescExtractor;
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
            var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
            var generatedInjectorTypeName = MetadataHelpers.GetGeneratedInjectorClassName(injectorInterfaceSymbol);

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

            IReadOnlyList<InjectorProviderDesc> providers = injectorMethods
                .Select(method => injectorProviderDescriptionExtractor.Extract(method, context))
                .Where(provider => provider != null)
                .Select(provider => provider!)
                .ToImmutableList();

            IReadOnlyList<ActivatorDesc> builders = injectorMethods
                .Select(method => injectorExtractorDescExtractor.Extract(method, context))
                .Where(builder => builder != null)
                .Select(builder => builder!)
                .ToImmutableList();

            IReadOnlyList<InjectorChildFactoryDesc> childFactories = injectorMethods
                .Select(method => injectorChildFactoryDescExtractor.Extract(method, context))
                .Where(childFactory => childFactory != null)
                .Select(childFactory => childFactory!)
                .ToImmutableList();

            return new InjectorDesc(
                injectorInterfaceType,
                generatedInjectorTypeName,
                specificationTypes,
                dependencyInterfaceTypes,
                providers,
                builders,
                childFactories,
                injectorInterfaceSymbol.Locations.First());
        }
    }
}
