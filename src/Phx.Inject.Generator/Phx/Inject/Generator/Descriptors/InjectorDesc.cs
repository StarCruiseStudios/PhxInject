// -----------------------------------------------------------------------------
//  <copyright file="InjectorDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Common;
using Phx.Inject.Generator.Model;

namespace Phx.Inject.Generator.Descriptors;

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

    public interface IBuilder {
        InjectorDesc Build(
            ITypeSymbol injectorInterfaceSymbol,
            DescGenerationContext context
        );
    }

    public class Builder : IBuilder {
        private readonly ActivatorDesc.IBuilder injectorBuilderDescBuilder;
        private readonly InjectorChildFactoryDesc.IBuilder injectorChildFactoryDescBuilder;
        private readonly InjectorProviderDesc.IBuilder injectorProviderDescriptionBuilder;

        public Builder(
            InjectorProviderDesc.IBuilder injectorProviderDescriptionBuilder,
            ActivatorDesc.IBuilder injectorBuilderDescBuilder,
            InjectorChildFactoryDesc.IBuilder injectorChildFactoryDescBuilder
        ) {
            this.injectorProviderDescriptionBuilder = injectorProviderDescriptionBuilder;
            this.injectorBuilderDescBuilder = injectorBuilderDescBuilder;
            this.injectorChildFactoryDescBuilder = injectorChildFactoryDescBuilder;
        }

        public Builder() : this(
            new InjectorProviderDesc.Builder(),
            new ActivatorDesc.Builder(),
            new InjectorChildFactoryDesc.Builder()) { }

        public InjectorDesc Build(
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
                .Select(method => injectorProviderDescriptionBuilder.Build(method, context))
                .Where(provider => provider != null)
                .Select(provider => provider!)
                .ToImmutableList();

            IReadOnlyList<ActivatorDesc> builders = injectorMethods
                .Select(method => injectorBuilderDescBuilder.Build(method, context))
                .Where(builder => builder != null)
                .Select(builder => builder!)
                .ToImmutableList();

            IReadOnlyList<InjectorChildFactoryDesc> childFactories = injectorMethods
                .Select(method => injectorChildFactoryDescBuilder.Build(method, context))
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
