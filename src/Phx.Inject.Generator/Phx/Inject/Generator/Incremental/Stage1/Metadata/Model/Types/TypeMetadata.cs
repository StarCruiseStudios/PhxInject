// -----------------------------------------------------------------------------
// <copyright file="TypeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Text;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

/// <summary>
///     Metadata representing a type including its namespace, name, and generic arguments.
/// </summary>
/// <param name="NamespaceName"> The namespace containing the type. </param>
/// <param name="BaseTypeName"> The base name of the type without generic arguments. </param>
/// <param name="TypeArguments"> The list of generic type arguments. </param>
/// <param name="Location"> The source location of the type. </param>
internal record TypeMetadata(
    string NamespaceName,
    string BaseTypeName,
    EquatableList<TypeMetadata> TypeArguments,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {

    /// <summary> Gets the full type name including generic arguments. </summary>
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
    
    /// <summary> Gets the base type name with its namespace. </summary>
    public string NamespacedBaseTypeName {
        get => $"{NamespaceName}.{BaseTypeName}";
    }
    
    /// <summary> Gets the full type name with its namespace. </summary>
    public string NamespacedName {
        get => $"{NamespaceName}.{TypeName}";
    }
    
    /// <summary> Compares this type metadata with another for equality. </summary>
    /// <param name="other"> The other type metadata to compare with. </param>
    /// <returns> True if the types are equal, false otherwise. </returns>
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
    
    /// <summary> Returns a string representation of the type. </summary>
    /// <returns> The namespaced full name of the type. </returns>
    public override string ToString() {
        return NamespacedName;
    }
    
    /// <summary> Creates a TypeMetadata instance from a Roslyn ITypeSymbol. </summary>
    /// <param name="typeSymbol"> The type symbol to convert. </param>
    /// <returns> A new TypeMetadata instance. </returns>
    public static TypeMetadata FromTypeSymbol(ITypeSymbol typeSymbol) {
        return typeSymbol.ToTypeModel();
    }
}

/// <summary>
///     Extension methods for converting Roslyn type symbols to metadata.
/// </summary>
internal static class TypeSymbolExtensions {
    /// <summary> Converts a type symbol to TypeMetadata. </summary>
    /// <param name="typeSymbol"> The type symbol to convert. </param>
    /// <returns> A new TypeMetadata instance. </returns>
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
    
    /// <summary> Converts a type symbol to QualifiedTypeMetadata with a qualifier. </summary>
    /// <param name="typeSymbol"> The type symbol to convert. </param>
    /// <param name="qualifierMetadata"> The qualifier metadata to apply. </param>
    /// <returns> A new QualifiedTypeMetadata instance. </returns>
    public static QualifiedTypeMetadata ToQualifiedTypeModel(this ITypeSymbol typeSymbol, IQualifierMetadata qualifierMetadata) {
        return new QualifiedTypeMetadata(typeSymbol.ToTypeModel(), qualifierMetadata);
    }
}
