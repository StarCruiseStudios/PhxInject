// -----------------------------------------------------------------------------
// <copyright file="TypeSymbolExtractor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Tests.Helpers;

internal static class TypeSymbolExtractor {
    public static IEnumerable<TSymbol> Extract<TSyntax, TSymbol>(
        IEnumerable<TSyntax> syntaxNodes,
        Compilation compilation
    )
        where TSyntax : SyntaxNode
        where TSymbol : ISymbol {
        foreach (var syntaxNode in syntaxNodes) {
            var syntaxTree = syntaxNode.SyntaxTree;
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            if (semanticModel.GetDeclaredSymbol(syntaxNode) is not TSymbol symbol) {
                continue;
            }

            yield return symbol;
        }
    }
}
