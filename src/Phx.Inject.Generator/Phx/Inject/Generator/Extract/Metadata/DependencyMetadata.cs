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
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record DependencyMetadata(
    TypeModel DependencyInterfaceType,
    TypeModel ContainingInjectorInterfaceType,
    IEnumerable<DependencyProviderMetadata> Providers,
    SpecDesc InstantiatedSpec,
    ITypeSymbol DependencyTypeSymbol
) : IDescriptor {
    public Location Location {
        get => DependencyInterfaceType.TypeSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        DependencyMetadata Extract(
            ITypeSymbol dependencySymbol,
            TypeModel containingInjectorInterfaceType,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor(
        DependencyProviderMetadata.IExtractor dependencyProviderExtractor,
        SpecDesc.IExtractor dependencySpecExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            DependencyProviderMetadata.Extractor.Instance,
            new SpecDesc.Extractor());

        public DependencyMetadata Extract(
            ITypeSymbol dependencySymbol,
            TypeModel containingInjectorInterfaceType,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                $"extracting dependency {containingInjectorInterfaceType} -> {dependencySymbol}",
                dependencySymbol,
                currentCtx => {
                    VerifyExtract(dependencySymbol, containingInjectorInterfaceType.Location, currentCtx);

                    var dependencyInterfaceType = dependencySymbol.ToTypeModel();
                    IReadOnlyList<DependencyProviderMetadata> providers = dependencySymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .Select(method =>
                            dependencyProviderExtractor.Extract(method, dependencyInterfaceType, currentCtx))
                        .ToImmutableList();

                    var instantiatedSpec =
                        dependencySpecExtractor.ExtractDependencySpec(dependencySymbol, providers, currentCtx);

                    return new DependencyMetadata(
                        dependencyInterfaceType,
                        containingInjectorInterfaceType,
                        providers,
                        instantiatedSpec,
                        dependencySymbol);
                });
        }

        private bool VerifyExtract(ITypeSymbol symbol, Location declarationLocation, IGeneratorContext? generatorCtx) {
            if (generatorCtx != null) {
                if (symbol.TypeKind != TypeKind.Interface) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency {symbol.Name} must be an interface.",
                        declarationLocation,
                        generatorCtx);
                }
            }

            return true;
        }
    }
}
