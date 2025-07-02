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
using Phx.Inject.Common;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract;

internal interface IDependencyExtractor {
    IReadOnlyList<DependencyDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    );
}

internal class DependencyExtractor : IDependencyExtractor {
    private readonly DependencyDesc.IExtractor dependencyDescExtractor;

    public DependencyExtractor(DependencyDesc.IExtractor dependencyDescExtractor) {
        this.dependencyDescExtractor = dependencyDescExtractor;
    }

    public DependencyExtractor() : this(
        new DependencyDesc.Extractor(new DependencyProviderDesc.Extractor())) { }

    public IReadOnlyList<DependencyDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    ) {
        return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
            .SelectMany(MetadataHelpers.GetDependencyTypes)
            .GroupBy(typeSymbol => typeSymbol, SymbolEqualityComparer.Default)
            .Select(group => group.First())
            .Where(symbol => IsDependencySymbol(symbol, context.GenerationContext))
            .SelectCatching(context.GenerationContext, type => dependencyDescExtractor.Extract(type, context))
            .ToImmutableList();
    }

    private static bool IsDependencySymbol(ITypeSymbol symbol, GeneratorExecutionContext context) {
        if (symbol.TypeKind != TypeKind.Interface) {
            throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                $"Dependency type {symbol.Name} must be an interface.",
                symbol.Locations.First());
        }

        return true;
    }
}
