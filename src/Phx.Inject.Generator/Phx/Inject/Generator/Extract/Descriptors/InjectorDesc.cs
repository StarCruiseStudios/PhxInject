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
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record InjectorDesc(
    TypeModel InjectorInterfaceType,
    TypeModel InjectorType,
    IEnumerable<TypeModel> SpecificationsTypes,
    TypeModel? DependencyInterfaceType,
    IEnumerable<InjectorProviderMetadata> Providers,
    IEnumerable<InjectorBuilderMetadata> Builders,
    IEnumerable<InjectorChildFactoryMetadata> ChildFactories,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        InjectorDesc Extract(
            ITypeSymbol injectorInterfaceSymbol,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor;
        private readonly InjectorAttributeMetadata.IExtractor injectorAttributeExtractor;
        private readonly InjectorBuilderMetadata.IExtractor injectorBuilderExtractor;
        private readonly InjectorChildFactoryMetadata.IExtractor injectorChildFactoryExtractor;
        private readonly InjectorProviderMetadata.IExtractor injectorProviderExtractor;

        public Extractor(
            InjectorProviderMetadata.IExtractor injectorProviderExtractor,
            InjectorBuilderMetadata.IExtractor injectorBuilderExtractor,
            InjectorChildFactoryMetadata.IExtractor injectorChildFactoryExtractor,
            DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor,
            InjectorAttributeMetadata.IExtractor injectorAttributeExtractor
        ) {
            this.injectorProviderExtractor = injectorProviderExtractor;
            this.injectorBuilderExtractor = injectorBuilderExtractor;
            this.injectorChildFactoryExtractor = injectorChildFactoryExtractor;
            this.dependencyAttributeExtractor = dependencyAttributeExtractor;
            this.injectorAttributeExtractor = injectorAttributeExtractor;
        }

        public Extractor() : this(
            InjectorProviderMetadata.Extractor.Instance,
            InjectorBuilderMetadata.Extractor.Instance,
            InjectorChildFactoryMetadata.Extractor.Instance,
            DependencyAttributeMetadata.Extractor.Instance,
            InjectorAttributeMetadata.Extractor.Instance) { }

        public InjectorDesc Extract(
            ITypeSymbol injectorInterfaceSymbol,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                "extracting injector",
                injectorInterfaceSymbol,
                currentCtx => {
                    var injectorLocation = injectorInterfaceSymbol.GetLocationOrDefault();
                    var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
                    if (!injectorAttributeExtractor.CanExtract(injectorInterfaceSymbol)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Type {injectorInterfaceSymbol} must declare an {InjectorAttributeMetadata.InjectorAttributeClassName}.",
                            injectorInterfaceSymbol.GetLocationOrDefault(),
                            currentCtx);
                    }

                    var injectorAttribute = injectorAttributeExtractor.Extract(injectorInterfaceSymbol, currentCtx);

                    var generatedInjectorTypeName =
                        injectorAttribute.GeneratedClassName?.AsValidIdentifier().StartUppercase()
                        ?? TypeModel.FromTypeSymbol(injectorInterfaceSymbol).GetInjectorClassName();
                    ;
                    var injectorType = injectorInterfaceType with {
                        BaseTypeName = generatedInjectorTypeName,
                        TypeArguments = ImmutableList<TypeModel>.Empty
                    };

                    var dependencyAttribute = dependencyAttributeExtractor.CanExtract(injectorInterfaceSymbol)
                        ? dependencyAttributeExtractor.Extract(injectorInterfaceSymbol, currentCtx)
                        : null;

                    IReadOnlyList<TypeModel> specificationTypes = injectorAttribute.Specifications
                        .AppendIfNotNull(dependencyAttribute?.DependencyType)
                        .ToImmutableList();

                    IReadOnlyList<IMethodSymbol> injectorMethods = injectorInterfaceSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .ToImmutableList();

                    IReadOnlyList<InjectorProviderMetadata> providers = injectorMethods
                        .Where(injectorProviderExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            methodSymbol =>
                                $"extracting injector provider method {injectorInterfaceType}.{methodSymbol.Name}",
                            methodSymbol => injectorProviderExtractor
                                .Extract(injectorInterfaceType, methodSymbol, currentCtx))
                        .ToImmutableList();

                    IReadOnlyList<InjectorBuilderMetadata> builders = injectorMethods
                        .Where(injectorBuilderExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            methodSymbol => $"extracting injector builder {injectorInterfaceType}.{methodSymbol.Name}",
                            method => injectorBuilderExtractor.Extract(injectorInterfaceType, method, currentCtx))
                        .ToImmutableList();

                    IReadOnlyList<InjectorChildFactoryMetadata> childFactories = injectorMethods
                        .Where(injectorChildFactoryExtractor.CanExtract)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            methodSymbol =>
                                $"extracting injector child factory {injectorType}.{methodSymbol.Name}",
                            methodSymbol =>
                                injectorChildFactoryExtractor.Extract(injectorInterfaceType, methodSymbol, currentCtx))
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
