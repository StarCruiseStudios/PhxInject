// -----------------------------------------------------------------------------
//  <copyright file="TestSourceGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    public class TestSourceGenerator : ISourceGenerator {
        private readonly Predicate<SyntaxNode> shouldCapture;
        private readonly Action<IReadOnlyList<SyntaxNode>> callback;

        public TestSourceGenerator(Predicate<SyntaxNode> shouldCapture, Action<IReadOnlyList<SyntaxNode>> callback) {
            this.shouldCapture = shouldCapture;
            this.callback = callback;
        }

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new TestSyntaxReceiver(shouldCapture));
        }

        public void Execute(GeneratorExecutionContext context) {
            var syntaxReceiver = context.SyntaxReceiver as TestSyntaxReceiver
                ?? throw new InvalidOperationException("Incorrect Syntax Receiver."); // This should never happen.

            callback(syntaxReceiver.CapturedNodes);
        }

        public class TestSyntaxReceiver : ISyntaxReceiver {
            private readonly Predicate<SyntaxNode> shouldCapture;
            private readonly List<SyntaxNode> capturedNodes = new();

            public IReadOnlyList<SyntaxNode> CapturedNodes { get { return capturedNodes; } }

            public TestSyntaxReceiver(Predicate<SyntaxNode> shouldCapture) {
                this.shouldCapture = shouldCapture;
            }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
                if (shouldCapture(syntaxNode)) {
                    capturedNodes.Add(syntaxNode);
                }
            }
        }

        public static IEnumerable<TSymbol> ExtractSymbols<TSyntax, TSymbol>(
            string code,
            Predicate<SyntaxNode> shouldCapture,
            string[]? additionalFiles = null)
        where TSyntax : SyntaxNode
        where TSymbol : ISymbol {
            var syntaxNodes = new List<TSyntax>();
            var sourceGenerator = new TestSourceGenerator(
                shouldCapture: shouldCapture,
                callback: (capturedNodes) => {
                    foreach (var node in capturedNodes) {
                        if (node is TSyntax syntaxNode) {
                            syntaxNodes.Add(syntaxNode);
                        }
                    }
                });
            var compilation = TestCompiler.CompileText(code, additionalFiles, sourceGenerator);
            var extractor = new TypeSymbolExtractor();
            return extractor.Extract<TSyntax, TSymbol>(syntaxNodes, compilation);
        }
    }
}
