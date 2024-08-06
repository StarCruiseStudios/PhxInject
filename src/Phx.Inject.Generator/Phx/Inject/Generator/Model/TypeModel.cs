// -----------------------------------------------------------------------------
//  <copyright file="TypeModel.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2023 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model {
    using System.Collections.Immutable;
    using System.Text;
    using Microsoft.CodeAnalysis;

    internal record TypeModel(
        string NamespaceName,
        string BaseTypeName,
        IImmutableList<TypeModel> TypeArguments,
        ITypeSymbol typeSymbol
    ) {
        public string TypeName {
            get {
                var builder = new StringBuilder(BaseTypeName);
                if (TypeArguments.Count > 0) {
                    builder.Append("<")
                        .Append(string.Join(",", TypeArguments.Select(argumentType => argumentType.QualifiedName)))
                        .Append(">");
                }

                return builder.ToString();
            }
        }

        public string QualifiedBaseTypeName => $"{NamespaceName}.{BaseTypeName}";

        public string QualifiedName => $"{NamespaceName}.{TypeName}";

        public override string ToString() {
            return QualifiedName;
        }

        public virtual bool Equals(TypeModel? other) {
            return QualifiedName == other?.QualifiedName;
        }

        public override int GetHashCode() {
            return QualifiedName.GetHashCode();
        }

        public static TypeModel FromTypeSymbol(ITypeSymbol typeSymbol) {
            var name = typeSymbol.Name;

            var typeArguments = (typeSymbol is INamedTypeSymbol namedTypeSymbol)
                ? namedTypeSymbol.TypeArguments
                    .Select(argumentType => FromTypeSymbol(argumentType))
                    .ToImmutableList()
                : ImmutableList<TypeModel>.Empty;

            if (typeSymbol.ContainingType != null) {
                var containingType = FromTypeSymbol(typeSymbol.ContainingType);
                name = $"{containingType.TypeName}.{name}";
            }

            return new TypeModel(
                typeSymbol.ContainingNamespace.ToString(),
                name,
                typeArguments,
                typeSymbol);
        }
    }
}
