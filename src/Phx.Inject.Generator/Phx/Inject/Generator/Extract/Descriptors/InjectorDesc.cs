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
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record InjectorDesc(
    TypeModel InjectorInterfaceType,
    TypeModel InjectorType,
    IEnumerable<TypeModel> SpecificationsTypes,
    TypeModel? DependencyInterfaceType,
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
        private readonly DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor;
        private readonly InjectorAttributeMetadata.IExtractor injectorAttributeExtractor;
        private readonly InjectorChildFactoryDesc.IExtractor injectorChildFactoryDescExtractor;
        private readonly InjectorProviderDesc.IExtractor injectorProviderDescriptionExtractor;

        public Extractor(
            InjectorProviderDesc.IExtractor injectorProviderDescriptionExtractor,
            ActivatorDesc.IExtractor activatorDescExtractor,
            InjectorChildFactoryDesc.IExtractor injectorChildFactoryDescExtractor,
            DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor,
            InjectorAttributeMetadata.IExtractor injectorAttributeExtractor
        ) {
            this.injectorProviderDescriptionExtractor = injectorProviderDescriptionExtractor;
            this.activatorDescExtractor = activatorDescExtractor;
            this.injectorChildFactoryDescExtractor = injectorChildFactoryDescExtractor;
            this.dependencyAttributeExtractor = dependencyAttributeExtractor;
            this.injectorAttributeExtractor = injectorAttributeExtractor;
        }

        public Extractor() : this(
            new InjectorProviderDesc.Extractor(),
            ActivatorDesc.Extractor.Instance,
            new InjectorChildFactoryDesc.Extractor(),
            DependencyAttributeMetadata.Extractor.Instance,
            InjectorAttributeMetadata.Extractor.Instance) { }

        public InjectorDesc Extract(
            ITypeSymbol injectorInterfaceSymbol,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(injectorInterfaceSymbol,
                currentCtx => {
                    var injectorLocation = injectorInterfaceSymbol.Locations.First();
                    var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
                    if (!injectorAttributeExtractor.CanExtract(injectorInterfaceSymbol)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Type {injectorInterfaceSymbol} must declare an {InjectorAttributeMetadata.InjectorAttributeClassName}.",
                            injectorInterfaceSymbol.Locations.First(),
                            currentCtx);
                    }

                    var injectorAttribute = injectorAttributeExtractor.Extract(injectorInterfaceSymbol)
                        .GetOrThrow(currentCtx)
                        .Also(_ => injectorAttributeExtractor.ValidateAttributedType(injectorInterfaceSymbol,
                            currentCtx));

                    var generatedInjectorTypeName =
                        injectorAttribute.GeneratedClassName?.AsValidIdentifier().StartUppercase()
                        ?? TypeModel.FromTypeSymbol(injectorInterfaceSymbol).GetInjectorClassName();
                    ;
                    var injectorType = injectorInterfaceType with {
                        BaseTypeName = generatedInjectorTypeName,
                        TypeArguments = ImmutableList<TypeModel>.Empty
                    };

                    var dependencyAttribute = dependencyAttributeExtractor.CanExtract(injectorInterfaceSymbol)
                        ? dependencyAttributeExtractor.Extract(injectorInterfaceSymbol)
                            .GetOrThrow(currentCtx)
                            .Also(_ => dependencyAttributeExtractor.ValidateAttributedType(
                                injectorInterfaceSymbol,
                                currentCtx))
                        : null;

                    IReadOnlyList<TypeModel> specificationTypes = injectorAttribute.Specifications
                        .AppendIfNotNull(dependencyAttribute?.DependencyType)
                        .ToImmutableList();

                    IReadOnlyList<IMethodSymbol> injectorMethods = injectorInterfaceSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .ToImmutableList();

                    IReadOnlyList<InjectorProviderDesc> providers = injectorMethods
                        .SelectCatching(
                            currentCtx.Aggregator,
                            methodSymbol =>
                                $"extracting injector provider method {injectorType}.{methodSymbol.Name}",
                            methodSymbol =>
                                injectorProviderDescriptionExtractor.Extract(methodSymbol, currentCtx))
                        .Where(provider => provider != null)
                        .Select(provider => provider!)
                        .ToImmutableList();

                    IReadOnlyList<ActivatorDesc> builders = injectorMethods
                        .SelectCatching(
                            currentCtx.Aggregator,
                            methodSymbol => $"extracting injector activator {injectorType}.{methodSymbol.Name}",
                            method => activatorDescExtractor.Extract(method, currentCtx))
                        .Where(builder => builder != null)
                        .Select(builder => builder!)
                        .ToImmutableList();

                    IReadOnlyList<InjectorChildFactoryDesc> childFactories = injectorMethods
                        .Where(injectorChildFactoryDescExtractor.IsInjectorChildFactory)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            methodSymbol =>
                                $"extracting injector child factory {injectorType}.{methodSymbol.Name}",
                            methodSymbol => injectorChildFactoryDescExtractor.Extract(methodSymbol, currentCtx))
                        .Select(childFactory => childFactory!)
                        .ToImmutableList();

                    return new InjectorDesc(
                        injectorInterfaceType,
                        injectorType,
                        specificationTypes,
                        dependencyAttribute?.DependencyType,
                        providers,
                        builders,
                        childFactories,
                        injectorLocation);
                });
        }
    }
}
