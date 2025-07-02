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
using Phx.Inject.Common;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract;

internal interface IInjectorExtractor {
    IReadOnlyList<InjectorDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    );
}

internal class InjectorExtractor : IInjectorExtractor {
    private readonly InjectorDesc.IExtractor injectorDescExtractor;

    public InjectorExtractor(InjectorDesc.IExtractor injectorDescExtractor) {
        this.injectorDescExtractor = injectorDescExtractor;
    }

    public InjectorExtractor() : this(new InjectorDesc.Extractor()) { }

    public IReadOnlyList<InjectorDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    ) {
        return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
            .Where(symbol => TypeHelpers.IsInjectorSymbol(symbol, context.GenerationContext))
            .SelectCatching(context.GenerationContext, symbol => injectorDescExtractor.Extract(symbol, context))
            .ToImmutableList();
    }
}
