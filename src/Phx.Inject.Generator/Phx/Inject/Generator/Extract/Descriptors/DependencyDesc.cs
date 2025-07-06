// -----------------------------------------------------------------------------
//  <copyright file="DependencyDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

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
                FactoryFabricationMode.Recurrent,
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

    public interface IExtractor {
        DependencyDesc Extract(
            ITypeSymbol dependencyInterfaceSymbol,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly DependencyProviderDesc.IExtractor dependencyProviderDescExtractor;

        public Extractor(DependencyProviderDesc.IExtractor dependencyProviderDescExtractor) {
            this.dependencyProviderDescExtractor = dependencyProviderDescExtractor;
        }

        public Extractor() : this(new DependencyProviderDesc.Extractor()) { }

        public DependencyDesc Extract(
            ITypeSymbol dependencyInterfaceSymbol,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(dependencyInterfaceSymbol);
            var dependencyInterfaceLocation = dependencyInterfaceSymbol.Locations.First();
            var dependencyInterfaceType = TypeModel.FromTypeSymbol(dependencyInterfaceSymbol);

            IReadOnlyList<DependencyProviderDesc> providers = dependencyInterfaceSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(method => dependencyProviderDescExtractor.Extract(method, currentCtx))
                .ToImmutableList();

            return new DependencyDesc(
                dependencyInterfaceType,
                providers,
                dependencyInterfaceLocation);
        }
    }
}
