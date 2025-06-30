// -----------------------------------------------------------------------------
//  <copyright file="InjectorExtractor.cs" company="Star Cruise Studios LLC">
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

internal class InjectorExtractor {
    private readonly InjectorDesc.IBuilder injectorDescBuilder;

    public InjectorExtractor(InjectorDesc.IBuilder injectorDescBuilder) {
        this.injectorDescBuilder = injectorDescBuilder;
    }

    public InjectorExtractor() : this(new InjectorDesc.Builder()) { }

    public IReadOnlyList<InjectorDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    ) {
        return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
            .Where(IsInjectorSymbol)
            .Select(symbol => injectorDescBuilder.Build(symbol, context))
            .ToImmutableList();
    }

    private static bool IsInjectorSymbol(ITypeSymbol symbol) {
        var injectorAttribute = symbol.GetInjectorAttribute();
        if (injectorAttribute == null) {
            return false;
        }

        if (symbol.TypeKind != TypeKind.Interface) {
            throw new InjectionException(
                Diagnostics.InvalidSpecification,
                $"Injector type {symbol.Name} must be an interface.",
                symbol.Locations.First());
        }

        return true;
    }
}
