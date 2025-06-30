// -----------------------------------------------------------------------------
//  <copyright file="TypeModel.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2023 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common.Model;

internal record TypeModel(
    string NamespaceName,
    string BaseTypeName,
    IReadOnlyList<TypeModel> TypeArguments,
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

    public string QualifiedBaseTypeName {
        get => $"{NamespaceName}.{BaseTypeName}";
    }

    public string QualifiedName {
        get => $"{NamespaceName}.{TypeName}";
    }

    public virtual bool Equals(TypeModel? other) {
        return QualifiedName == other?.QualifiedName;
    }

    public override string ToString() {
        return QualifiedName;
    }

    public override int GetHashCode() {
        return QualifiedName.GetHashCode();
    }

    public static TypeModel FromTypeSymbol(ITypeSymbol typeSymbol) {
        var name = typeSymbol.Name;

        IReadOnlyList<TypeModel> typeArguments = typeSymbol is INamedTypeSymbol namedTypeSymbol
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
