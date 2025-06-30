// -----------------------------------------------------------------------------
//  <copyright file="DependencyDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Model;

namespace Phx.Inject.Generator.Descriptors;

internal record DependencyDesc(
    TypeModel DependencyInterfaceType,
    IEnumerable<DependencyProviderDesc> Providers,
    Location Location
) : IDescriptor {
    public SpecDesc GetSpecDesc() {
        IReadOnlyList<SpecFactoryDesc> factories = Providers.Select(provider => new SpecFactoryDesc(
                provider.ProvidedType,
                provider.ProviderMethodName,
                SpecFactoryMemberType.Method,
                ImmutableList<QualifiedTypeModel>.Empty,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                SpecFactoryMethodFabricationMode.Recurrent,
                provider.isPartial,
                provider.Location))
            .ToImmutableList();

        return new SpecDesc(
            DependencyInterfaceType,
            SpecInstantiationMode.Instantiated,
            factories,
            ImmutableList<SpecBuilderDesc>.Empty,
            ImmutableList<SpecLinkDesc>.Empty,
            Location);
    }

    public interface IBuilder {
        DependencyDesc Build(
            ITypeSymbol dependencyInterfaceSymbol,
            DescGenerationContext context
        );
    }

    public class Builder : IBuilder {
        private readonly DependencyProviderDesc.IBuilder dependencyProviderDescBuilder;

        public Builder(DependencyProviderDesc.IBuilder dependencyProviderDescBuilder) {
            this.dependencyProviderDescBuilder = dependencyProviderDescBuilder;
        }

        public DependencyDesc Build(
            ITypeSymbol dependencyInterfaceSymbol,
            DescGenerationContext context
        ) {
            var dependencyInterfaceLocation = dependencyInterfaceSymbol.Locations.First();
            var dependencyInterfaceType = TypeModel.FromTypeSymbol(dependencyInterfaceSymbol);

            IReadOnlyList<DependencyProviderDesc> providers = dependencyInterfaceSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(method => dependencyProviderDescBuilder.Build(method, context))
                .ToImmutableList();

            return new DependencyDesc(
                dependencyInterfaceType,
                providers,
                dependencyInterfaceLocation);
        }
    }
}
