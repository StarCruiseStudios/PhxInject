// -----------------------------------------------------------------------------
//  <copyright file="ModelExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ModelExtractor<TModel> : IModelExtractor<TModel> where TModel : class {
        private ISymbolRecognizer<TModel> Recognizer { get; }
        private IModelBuilder<TModel> ModelBuilder { get; }

        public ModelExtractor(ISymbolRecognizer<TModel> recognizer, IModelBuilder<TModel> modelBuilder) {
            Recognizer = recognizer;
            ModelBuilder = modelBuilder;
        }

        public IReadOnlyList<TModel> Extract(
                IEnumerable<TypeDeclarationSyntax> syntaxNodes,
                GeneratorExecutionContext context
        ) {
            var models = new List<TModel>();
            foreach (var syntaxNode in syntaxNodes) {
                var syntaxTree = syntaxNode.SyntaxTree;
                var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                if (semanticModel.GetDeclaredSymbol(syntaxNode) is not ITypeSymbol symbol) {
                    continue;
                }

                if (Recognizer.IsExpectedSymbol(symbol)) {
                    models.Add(ModelBuilder.Build(symbol));
                }
            }

            return models;
        }
    }
}
