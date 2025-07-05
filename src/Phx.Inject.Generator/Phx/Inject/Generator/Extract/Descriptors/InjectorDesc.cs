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
using Phx.Inject.Common.Exceptions;
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
            ExtractorContext extractorCtx
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
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(injectorInterfaceSymbol);
            return ExceptionAggregator.Try(
                $"extracting injector for type {injectorInterfaceSymbol.Name}",
                currentCtx,
                exceptionAggregator => {
                    var injectorLocation = injectorInterfaceSymbol.Locations.First();
                    var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
                    var generatedInjectorTypeName =
                        MetadataHelpers.GetGeneratedInjectorClassName(injectorInterfaceSymbol, currentCtx);
                    var injectorType = injectorInterfaceType with {
                        BaseTypeName = generatedInjectorTypeName,
                        TypeArguments = ImmutableList<TypeModel>.Empty
                    };
                    IReadOnlyList<TypeModel> dependencyInterfaceTypes =
                        MetadataHelpers.TryGetDependencyType(injectorInterfaceSymbol)
                            .GetOrThrow(currentCtx)
                            .Let(dependencyInterfaceSymbol => dependencyInterfaceSymbol != null
                                ? ImmutableList.Create(TypeModel.FromTypeSymbol(dependencyInterfaceSymbol))
                                : ImmutableList<TypeModel>.Empty);

                    IReadOnlyList<TypeModel> specificationTypes = MetadataHelpers
                        .TryGetInjectorSpecificationTypes(injectorInterfaceSymbol, currentCtx)
                        .Select(TypeModel.FromTypeSymbol)
                        .Concat(dependencyInterfaceTypes)
                        .ToImmutableList();

                    IReadOnlyList<IMethodSymbol> injectorMethods = injectorInterfaceSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .ToImmutableList();

                    IReadOnlyList<InjectorProviderDesc> providers = injectorMethods
                        .SelectCatching(
                            exceptionAggregator,
                            methodSymbol => $"extracting injector provider method {injectorType}.{methodSymbol.Name}",
                            methodSymbol => injectorProviderDescriptionExtractor.Extract(methodSymbol, currentCtx))
                        .Where(provider => provider != null)
                        .Select(provider => provider!)
                        .ToImmutableList();

                    IReadOnlyList<ActivatorDesc> builders = injectorMethods
                        .SelectCatching(
                            exceptionAggregator,
                            methodSymbol => $"extracting injector activator {injectorType}.{methodSymbol.Name}",
                            method => activatorDescExtractor.Extract(method, currentCtx))
                        .Where(builder => builder != null)
                        .Select(builder => builder!)
                        .ToImmutableList();

                    IReadOnlyList<InjectorChildFactoryDesc> childFactories = injectorMethods
                        .SelectCatching(
                            exceptionAggregator,
                            methodSymbol => $"extracting injector child factory {injectorType}.{methodSymbol.Name}",
                            methodSymbol => injectorChildFactoryDescExtractor.Extract(methodSymbol, currentCtx))
                        .Where(childFactory => childFactory != null)
                        .Select(childFactory => childFactory!)
                        .ToImmutableList();

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
