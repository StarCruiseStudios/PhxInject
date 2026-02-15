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
///     Immutable representation of a .NET type's identity for incremental source generation.
/// </summary>
/// <param name="NamespaceName">The fully-qualified namespace containing this type.</param>
/// <param name="BaseTypeName">The type name without namespace or generic arity, potentially including nested type path.</param>
/// <param name="TypeArguments">Ordered collection of generic type arguments. Empty for non-generic types.</param>
/// <param name="Location">Source location wrapped in GeneratorIgnored to exclude from equality/hashing.</param>
/// <remarks>
///     Designed as a value-semantic cache key for Roslyn's incremental generator pipeline.
///     Overrides Equals/GetHashCode to exclude Location from comparisons, preventing cache
///     invalidation from whitespace/comment changes. Nested types incorporate the containing
///     type path in BaseTypeName (e.g., "Outer.Inner").
/// </remarks>
internal record TypeMetadata(
    string NamespaceName,
    string BaseTypeName,
    EquatableList<TypeMetadata> TypeArguments,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {

    /// <summary>
    ///     Gets the complete type name with generic arguments in C# syntax (e.g., "List&lt;String&gt;").
    /// </summary>
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
    
    /// <summary>
    ///     Gets the fully-qualified base type name without generic arguments
    ///     (e.g., "System.Collections.Generic.List").
    /// </summary>
    public string NamespacedBaseTypeName {
        get => $"{NamespaceName}.{BaseTypeName}";
    }
    
    /// <summary>
    ///     Gets the fully-qualified type name including namespace and generic arguments.
    /// </summary>
    public string NamespacedName {
        get => $"{NamespaceName}.{TypeName}";
    }
    
    /// <summary>
    ///     Compares this type metadata with another based on semantic identity, excluding location.
    /// </summary>
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
    
    /// <summary>
    ///     Computes a stable hash code based on semantic identity, excluding location.
    /// </summary>
    public override int GetHashCode() {
        var hash = 17;
        hash = hash * 31 + NamespaceName.GetHashCode();
        hash = hash * 31 + BaseTypeName.GetHashCode();
        foreach (var typeArgument in TypeArguments) {
            hash = hash * 31 + typeArgument.GetHashCode();
        }
        return hash;
    }
    
    /// <summary>
    ///     Returns the fully-qualified type name.
    /// </summary>
    public override string ToString() {
        return NamespacedName;
    }
    
    /// <summary>
    ///     Factory method to construct TypeMetadata from Roslyn's symbol representation.
    /// </summary>
    public static TypeMetadata FromTypeSymbol(ITypeSymbol typeSymbol) {
        return typeSymbol.ToTypeModel();
    }
}

/// <summary>
///     Provides conversions from Roslyn's symbol model to immutable metadata representations.
/// </summary>
internal static class TypeSymbolExtensions {
    /// <summary>
    ///     Converts a Roslyn type symbol to an immutable TypeMetadata instance.
    /// </summary>
    /// <remarks>
    ///     Nested types are flattened into BaseTypeName (e.g., "Outer.Inner").
    ///     Generic type arguments are recursively converted.
    /// </remarks>
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
    
    /// <summary>
    ///     Converts a type symbol to a QualifiedTypeMetadata by combining it with a qualifier.
    /// </summary>
    public static QualifiedTypeMetadata ToQualifiedTypeModel(this ITypeSymbol typeSymbol, IQualifierMetadata qualifierMetadata) {
        return new QualifiedTypeMetadata(typeSymbol.ToTypeModel(), qualifierMetadata);
    }
}
