// -----------------------------------------------------------------------------
//  <copyright file="IModelExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface IModelExtractor<TModel> where TModel : class {
        IReadOnlyList<TModel> Extract(
            IEnumerable<TypeDeclarationSyntax> syntaxNodes,
            GeneratorExecutionContext context);
    }
}
