// -----------------------------------------------------------------------------
//  <copyright file="TypeModel.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model {
    using Microsoft.CodeAnalysis;

    internal record TypeModel(string NamespaceName, string TypeName) {
        public string QualifiedName => $"{NamespaceName}.{TypeName}";

        public override string ToString() {
            return QualifiedName;
        }

        public static TypeModel FromTypeSymbol(ITypeSymbol typeSymbol) {
            var name = typeSymbol.Name;
            if (typeSymbol.ContainingType != null) {
                var containingType = FromTypeSymbol(typeSymbol.ContainingType);
                name = $"{containingType.TypeName}.{name}";
            }

            return new TypeModel(
                    typeSymbol.ContainingNamespace.ToString(),
                    name);
        }
    }
}
