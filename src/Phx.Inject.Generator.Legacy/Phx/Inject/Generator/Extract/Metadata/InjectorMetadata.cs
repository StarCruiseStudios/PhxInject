// -----------------------------------------------------------------------------
// <copyright file="InjectorMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record InjectorMetadata(
    TypeModel InjectorInterfaceType,
    TypeModel InjectorType,
    IEnumerable<TypeModel> SpecificationsTypes,
    TypeModel? DependencyInterfaceType,
    IEnumerable<InjectorProviderMetadata> Providers,
    IEnumerable<InjectorBuilderMetadata> Builders,
    IEnumerable<InjectorChildFactoryMetadata> ChildFactories,
    InjectorAttributeMetadata InjectorAttribute,
    DependencyAttributeMetadata? DependencyAttribute,
    ITypeSymbol InjectorInterfaceTypeSymbol
) : IMetadata {
    public Location Location {
        get => InjectorInterfaceTypeSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        bool CanExtract(ITypeSymbol providerMethodSymbol);
        InjectorMetadata Extract(
            ITypeSymbol injectorInterfaceSymbol,
            ExtractorContext parentCtx
        );
    }

    public class Extractor(
        InjectorProviderMetadata.IExtractor injectorProviderExtractor,
        InjectorBuilderMetadata.IExtractor injectorBuilderExtractor,
        InjectorChildFactoryMetadata.IExtractor injectorChildFactoryExtractor,
        DependencyAttributeMetadata.IExtractor dependencyAttributeExtractor,
        InjectorAttributeMetadata.IExtractor injectorAttributeExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            InjectorProviderMetadata.Extractor.Instance,
            InjectorBuilderMetadata.Extractor.Instance,
            InjectorChildFactoryMetadata.Extractor.Instance,
            DependencyAttributeMetadata.Extractor.Instance,
            InjectorAttributeMetadata.Extractor.Instance);

        public bool CanExtract(ITypeSymbol providerMethodSymbol) {
            return VerifyExtract(providerMethodSymbol, null);
        }

        public InjectorMetadata Extract(
            ITypeSymbol injectorInterfaceSymbol,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting injector {injectorInterfaceSymbol}",
                injectorInterfaceSymbol,
                currentCtx => {
                    VerifyExtract(injectorInterfaceSymbol, currentCtx);

                    var injectorInterfaceType = injectorInterfaceSymbol.ToTypeModel();
                    var injectorAttribute = injectorAttributeExtractor.Extract(injectorInterfaceSymbol, currentCtx);

                    var generatedInjectorTypeName =
                        injectorAttribute.GeneratedClassName?.AsValidIdentifier().StartUppercase()
                        ?? injectorInterfaceType.GetInjectorClassName();

                    var injectorType = injectorInterfaceType with {
                        BaseTypeName = generatedInjectorTypeName,
                        TypeArguments = ImmutableList<TypeModel>.Empty
                    };

                    var dependencyAttribute = dependencyAttributeExtractor.CanExtract(injectorInterfaceSymbol)
                        ? dependencyAttributeExtractor.Extract(injectorInterfaceSymbol, currentCtx)
                        : null;
                    var dependencyType = dependencyAttribute?.DependencyType;

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

                    return new InjectorMetadata(
                        injectorInterfaceType,
                        injectorType,
                        specificationTypes,
                        dependencyType,
                        providers,
                        builders,
                        childFactories,
                        injectorAttribute,
                        dependencyAttribute,
                        injectorInterfaceSymbol);
                });
        }

        private bool VerifyExtract(ITypeSymbol injectorInterfaceSymbol, IGeneratorContext? currentCtx) {
            if (!injectorAttributeExtractor.CanExtract(injectorInterfaceSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Child injector factory must declare an {InjectorAttributeMetadata.InjectorAttributeClassName}.",
                        injectorInterfaceSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (injectorInterfaceSymbol is not ITypeSymbol {
                        TypeKind: TypeKind.Interface,
                        DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                    }
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Injector type {injectorInterfaceSymbol.Name} must be a public or internal interface.",
                        injectorInterfaceSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }
}
