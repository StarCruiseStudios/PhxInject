// -----------------------------------------------------------------------------
//  <copyright file="DependencyMetadata.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record DependencyMetadata(
    TypeModel DependencyInterfaceType,
    TypeModel ContainingInjectorInterfaceType,
    IEnumerable<DependencyProviderMetadata> Providers,
    SpecificationAttributeMetadata SpecificationAttributeMetadata,
    ITypeSymbol DependencyTypeSymbol
) : IMetadata {
    public Location Location {
        get => DependencyInterfaceType.TypeSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        DependencyMetadata Extract(
            ITypeSymbol dependencySymbol,
            TypeModel containingInjectorInterfaceType,
            ExtractorContext parentCtx
        );
    }

    public class Extractor(
        SpecificationAttributeMetadata.IExtractor specificationAttributeExtractor,
        DependencyProviderMetadata.IExtractor dependencyProviderExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            SpecificationAttributeMetadata.Extractor.Instance,
            DependencyProviderMetadata.Extractor.Instance
        );

        public DependencyMetadata Extract(
            ITypeSymbol dependencySymbol,
            TypeModel containingInjectorInterfaceType,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting dependency {containingInjectorInterfaceType} -> {dependencySymbol}",
                dependencySymbol,
                currentCtx => {
                    VerifyExtract(dependencySymbol, containingInjectorInterfaceType.Location, currentCtx);

                    var specificationAttribute = specificationAttributeExtractor
                        .Extract(dependencySymbol, currentCtx);
                    var dependencyInterfaceType = dependencySymbol.ToTypeModel();
                    var providerMembers = dependencySymbol.GetMembers();
                    var providerMethods = providerMembers
                        .OfType<IMethodSymbol>()
                        .Where(dependencyProviderExtractor.CanExtract)
                        .Select(method =>
                            dependencyProviderExtractor.Extract(method, currentCtx));

                    var providerProperties = providerMembers
                        .OfType<IPropertySymbol>()
                        .Where(dependencyProviderExtractor.CanExtract)
                        .Select(property =>
                            dependencyProviderExtractor.Extract(property, currentCtx));
                    IReadOnlyList<DependencyProviderMetadata> providers = providerMethods
                        .Concat(providerProperties)
                        .ToImmutableList();

                    return new DependencyMetadata(
                        dependencyInterfaceType,
                        containingInjectorInterfaceType,
                        providers,
                        specificationAttribute,
                        dependencySymbol);
                });
        }

        private bool VerifyExtract(ITypeSymbol symbol, Location declarationLocation, IGeneratorContext? currentCtx) {
            if (currentCtx != null) {
                if (symbol.TypeKind != TypeKind.Interface) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency {symbol.Name} must be an interface.",
                        declarationLocation,
                        currentCtx);
                }

                if (!specificationAttributeExtractor.CanExtract(symbol)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency {symbol.Name} must have a {SpecificationAttributeMetadata.SpecificationAttributeClassName}.",
                        symbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }
}
