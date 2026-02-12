// -----------------------------------------------------------------------------
// <copyright file="TypeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

internal record TypeMetadata(
    string NamespaceName,
    string BaseTypeName,
    EquatableList<TypeMetadata> TypeArguments,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {

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
    
    public virtual bool Equals(TypeMetadata? other) {
        if (other is null) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }
        
        return NamespaceName == other.NamespaceName
            && BaseTypeName == other.BaseTypeName
            && TypeArguments.SequenceEqual(other.TypeArguments);
    }
    
    public override int GetHashCode() {
        var hash = 17;
        hash = hash * 31 + NamespaceName.GetHashCode();
        hash = hash * 31 + BaseTypeName.GetHashCode();
        foreach (var typeArgument in TypeArguments) {
            hash = hash * 31 + typeArgument.GetHashCode();
        }
        return hash;
    }
    
    public override string ToString() {
        return NamespacedName;
    }
    
    public static TypeMetadata FromTypeSymbol(ITypeSymbol typeSymbol) {
        return typeSymbol.ToTypeModel();
    }
}

internal static class TypeSymbolExtensions {
    public static TypeMetadata ToTypeModel(this ITypeSymbol typeSymbol) {
        var name = typeSymbol.Name;

        EquatableList<TypeMetadata> typeArguments = typeSymbol is INamedTypeSymbol namedTypeSymbol
            ? namedTypeSymbol.TypeArguments
                .Select(argumentType => argumentType.ToTypeModel())
                .ToEquatableList()
            : EquatableList<TypeMetadata>.Empty;

        if (typeSymbol.ContainingType != null) {
            var containingType = typeSymbol.ContainingType.ToTypeModel();
            name = $"{containingType.TypeName}.{name}";
        }

        return new TypeMetadata(
            typeSymbol.ContainingNamespace.ToString(),
            name,
            typeArguments,
            typeSymbol.Locations.FirstOrDefault().GeneratorIgnored());
    }
    
    public static QualifiedTypeMetadata ToQualifiedTypeModel(this ITypeSymbol typeSymbol, IQualifierMetadata qualifierMetadata) {
        return new QualifiedTypeMetadata(typeSymbol.ToTypeModel(), qualifierMetadata);
    }
}
