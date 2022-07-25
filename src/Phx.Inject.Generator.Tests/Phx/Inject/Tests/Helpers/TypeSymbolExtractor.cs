// -----------------------------------------------------------------------------
//  <copyright file="TypeSymbolExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Helpers {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class TypeSymbolExtractor {
        public IEnumerable<TSymbol> Extract<TSyntax, TSymbol>(
            IEnumerable<TSyntax> syntaxNodes,
            Compilation compilation)
        where TSyntax : SyntaxNode
        where TSymbol : ISymbol {
            foreach (var syntaxNode in syntaxNodes) {
                SyntaxTree syntaxTree = syntaxNode.SyntaxTree;
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                if (semanticModel.GetDeclaredSymbol(syntaxNode) is not TSymbol symbol) {
                    continue;
                }

                yield return symbol;
            }
        }
    }
}
