// -----------------------------------------------------------------------------
//  <copyright file="TypeModel.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract.Model {
    using Microsoft.CodeAnalysis;

    internal record TypeModel(string NamespaceName, string Name) {
        public string QualifiedName => $"{NamespaceName}.{Name}";
    }

    internal static class ITypeSymbolExtensions {
        public static TypeModel ToTypeModel(this ITypeSymbol typeSymbol) {
            return new TypeModel(
                    typeSymbol.ContainingNamespace.ToString(),
                    typeSymbol.Name);
        }
    }
}
