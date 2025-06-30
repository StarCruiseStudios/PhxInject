// -----------------------------------------------------------------------------
//  <copyright file="DependencyExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Generator.Common;

namespace Phx.Inject.Generator.Descriptors;

internal class DependencyExtractor {
    private readonly DependencyDesc.IBuilder dependencyDescBuilder;

    public DependencyExtractor(DependencyDesc.IBuilder dependencyDescBuilder) {
        this.dependencyDescBuilder = dependencyDescBuilder;
    }

    public DependencyExtractor() : this(
        new DependencyDesc.Builder(new DependencyProviderDesc.Builder())) { }

    public IReadOnlyList<DependencyDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    ) {
        return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
            .SelectMany(MetadataHelpers.GetDependencyTypes)
            .GroupBy(typeSymbol => typeSymbol, SymbolEqualityComparer.Default)
            .Select(group => group.First())
            .Where(IsDependencySymbol)
            .Select(type => dependencyDescBuilder.Build(type, context))
            .ToImmutableList();
    }

    private static bool IsDependencySymbol(ITypeSymbol symbol) {
        if (symbol.TypeKind != TypeKind.Interface) {
            throw new InjectionException(
                Diagnostics.InvalidSpecification,
                $"Dependency type {symbol.Name} must be an interface.",
                symbol.Locations.First());
        }

        return true;
    }
}
