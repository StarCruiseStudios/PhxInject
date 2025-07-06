// -----------------------------------------------------------------------------
//  <copyright file="DependencyDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record DependencyDesc(
    TypeModel DependencyInterfaceType,
    TypeModel ContainingInjectorInterfaceType,
    IEnumerable<DependencyProviderDesc> Providers,
    SpecDesc InstantiatedSpecDesc
) : IDescriptor {
    public Location Location => DependencyInterfaceType.TypeSymbol.Locations.First();
    
    public static void RequireDependency(
        ITypeSymbol symbol,
        Location declarationLocation,
        IGeneratorContext generatorCtx
    ) {
        ExceptionAggregator.Try(
            "Validating dependency",
            generatorCtx,
            _ => {
                if (symbol.TypeKind != TypeKind.Interface) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency {symbol.Name} must be an interface.",
                        declarationLocation,
                        generatorCtx);
                }
            });
    }

    public interface IExtractor {
        DependencyDesc Extract(
            ITypeSymbol symbol,
            TypeModel containingInjectorInterfaceType,
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
            ITypeSymbol symbol,
            TypeModel containingInjectorInterfaceType,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(symbol);
            RequireDependency(symbol, containingInjectorInterfaceType.Location, currentCtx);
            
            var dependencyInterfaceType = TypeModel.FromTypeSymbol(symbol);
            IReadOnlyList<DependencyProviderDesc> providers = symbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(method => dependencyProviderDescExtractor.Extract(method, dependencyInterfaceType, currentCtx))
                .ToImmutableList();
            
            IReadOnlyList<SpecFactoryDesc> specFactories = providers.Select(provider => new SpecFactoryDesc(
                    provider.ProvidedType,
                    provider.ProviderMethodName,
                    SpecFactoryMemberType.Method,
                    ImmutableList<QualifiedTypeModel>.Empty,
                    ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                    FactoryFabricationMode.Recurrent,
                    provider.IsPartial,
                    provider.Location))
                .ToImmutableList();

            var instantiatedSpecDesc = // TODO: Make this use the SpecDesc Exctractor instead
                                       // new SpecDesc.Extractor().Extract(symbol, currentCtx);
                new SpecDesc(
                    dependencyInterfaceType,
                    SpecInstantiationMode.Instantiated,
                    specFactories,
                    ImmutableList<SpecBuilderDesc>.Empty,
                    ImmutableList<SpecLinkDesc>.Empty);
            
            return new DependencyDesc(
                dependencyInterfaceType,
                containingInjectorInterfaceType,
                providers,
                instantiatedSpecDesc);
        }
    }
}
