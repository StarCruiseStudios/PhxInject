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
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Common.Model;

internal record TypeModel(
    string NamespaceName,
    string BaseTypeName,
    IReadOnlyList<TypeModel> TypeArguments,
    ITypeSymbol TypeSymbol
) {
    public Location Location {
        get => TypeSymbol.GetLocationOrDefault();
    }

    public string TypeName {
        get {
            var builder = new StringBuilder(BaseTypeName);
            if (TypeArguments.Count > 0) {
                builder.Append("<")
                    .Append(string.Join(",", TypeArguments.Select(argumentType => argumentType.NamespacedName)))
                    .Append(">");
            }

            return builder.ToString();
        }
    }

    public string NamespacedBaseTypeName {
        get => $"{NamespaceName}.{BaseTypeName}";
    }

    public string NamespacedName {
        get => $"{NamespaceName}.{TypeName}";
    }

    public virtual bool Equals(TypeModel? other) {
        return NamespacedName == other?.NamespacedName;
    }

    public override string ToString() {
        return NamespacedName;
    }

    public override int GetHashCode() {
        return NamespacedName.GetHashCode();
    }

    public static TypeModel FromTypeSymbol(ITypeSymbol typeSymbol) {
        return typeSymbol.ToTypeModel();
    }
}

internal static class TypeSymbolExtensions {
    public static TypeModel ToTypeModel(this ITypeSymbol typeSymbol) {
        var name = typeSymbol.Name;

        IReadOnlyList<TypeModel> typeArguments = typeSymbol is INamedTypeSymbol namedTypeSymbol
            ? namedTypeSymbol.TypeArguments
                .Select(argumentType => argumentType.ToTypeModel())
                .ToImmutableList()
            : ImmutableList<TypeModel>.Empty;

        if (typeSymbol.ContainingType != null) {
            var containingType = typeSymbol.ContainingType.ToTypeModel();
            name = $"{containingType.TypeName}.{name}";
        }

        return new TypeModel(
            typeSymbol.ContainingNamespace.ToString(),
            name,
            typeArguments,
            typeSymbol);
    }

    public static QualifiedTypeModel ToQualifiedTypeModel(this ITypeSymbol typeSymbol, QualifierMetadata qualifier) {
        return new QualifiedTypeModel(
            typeSymbol.ToTypeModel(),
            qualifier);
    }
}
