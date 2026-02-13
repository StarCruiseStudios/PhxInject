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
/// <param name="NamespaceName">
///     The fully-qualified namespace containing this type (e.g., "System.Collections.Generic").
/// </param>
/// <param name="BaseTypeName">
///     The type name without namespace or generic arity, potentially including nested type path
///     (e.g., "List" or "OuterClass.InnerClass"). Generic arity indicators like `1 are excluded.
/// </param>
/// <param name="TypeArguments">
///     Ordered collection of generic type arguments. Empty for non-generic types.
///     Must use EquatableList for proper structural equality in incremental compilation caching.
/// </param>
/// <param name="Location">
///     Source location wrapped in GeneratorIgnored to exclude from equality/hashing.
///     Location data is diagnostic metadata only and must not affect incremental cache keys.
/// </param>
/// <remarks>
///     <para>Design Rationale - Immutability for Incremental Compilation:</para>
///     <para>
///     This record is designed as a value-semantic cache key for Roslyn's incremental generator pipeline.
///     Records provide structural equality by default, but we override Equals/GetHashCode to exclude
///     Location from comparisons, ensuring that changes to whitespace or comments don't invalidate
///     the cache when the type's semantic identity is unchanged.
///     </para>
///     
///     <para>Nested Type Handling Strategy:</para>
///     <para>
///     Nested types are represented by incorporating the containing type's full name into BaseTypeName
///     (e.g., "Outer.Inner.Nested"). This design choice simplifies name generation and equality comparison
///     while maintaining a flat structure. Alternative approaches (parent references, hierarchical structures)
///     would complicate equality semantics and increase cache invalidation sensitivity.
///     </para>
///     
///     <para>Performance Considerations:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             GetHashCode is cached implicitly by record semantics, avoiding recomputation
///             </description>
///         </item>
///         <item>
///             <description>
///             String properties (TypeName, NamespacedName) compute lazily but are not cached;
///             caller should cache if used repeatedly in hot paths
///             </description>
///         </item>
///         <item>
///             <description>
///             TypeArguments uses EquatableList for O(n) equality checks instead of O(n²) reference comparison
///             </description>
///         </item>
///     </list>
///     
///     <para>When to Use:</para>
///     <para>
///     Use TypeMetadata when you need a stable, comparable representation of a type's identity
///     for incremental caching, code generation, or cross-reference resolution. Do NOT use for
///     runtime type operations or when you need the actual ITypeSymbol for semantic analysis.
///     </para>
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
    /// <remarks>
    ///     For non-generic types, returns BaseTypeName unchanged. For generic types, constructs
    ///     syntax like "Dictionary&lt;System.String,System.Int32&gt;" using fully-qualified type arguments.
    ///     This format is suitable for generated C# code but not for display purposes.
    /// </remarks>
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
    /// <remarks>
    ///     Useful when you need the generic type definition rather than a specific instantiation.
    ///     For non-generic types, this is equivalent to NamespacedName.
    /// </remarks>
    public string NamespacedBaseTypeName {
        get => $"{NamespaceName}.{BaseTypeName}";
    }
    
    /// <summary>
    ///     Gets the fully-qualified type name including namespace and generic arguments
    ///     (e.g., "System.Collections.Generic.List&lt;System.String&gt;").
    /// </summary>
    /// <remarks>
    ///     This is the canonical identifier for the type and should be used in generated code
    ///     to avoid ambiguity. Equivalent to NamespacedBaseTypeName for non-generic types.
    /// </remarks>
    public string NamespacedName {
        get => $"{NamespaceName}.{TypeName}";
    }
    
    /// <summary>
    ///     Compares this type metadata with another based on semantic identity, excluding location.
    /// </summary>
    /// <param name="other">The type to compare against, or null.</param>
    /// <returns>
    ///     True if both types represent the same semantic type (same namespace, name, and type arguments).
    ///     False if other is null or represents a different type.
    /// </returns>
    /// <remarks>
    ///     This override is critical for incremental compilation caching. By excluding Location from
    ///     the comparison, we ensure that semantically identical types from different source locations
    ///     or compilation runs are treated as equal, minimizing unnecessary cache invalidation.
    ///     The structural equality of TypeArguments is handled by EquatableList's IEnumerable equality.
    /// </remarks>
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
    /// <returns>
    ///     A hash code derived from namespace, base name, and type arguments that remains
    ///     consistent across compilation runs for semantically identical types.
    /// </returns>
    /// <remarks>
    ///     Uses FNV-1a style hashing with prime multipliers for good distribution.
    ///     Must remain synchronized with Equals() implementation to maintain the contract
    ///     that equal objects have equal hash codes. Location is deliberately excluded
    ///     to ensure cache stability in incremental compilation scenarios.
    /// </remarks>
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
    ///     Returns the fully-qualified type name for diagnostic and debugging purposes.
    /// </summary>
    /// <returns>The same value as NamespacedName.</returns>
    public override string ToString() {
        return NamespacedName;
    }
    
    /// <summary>
    ///     Factory method to construct TypeMetadata from Roslyn's symbol representation.
    /// </summary>
    /// <param name="typeSymbol">
    ///     The Roslyn type symbol to extract metadata from. Must not be null.
    /// </param>
    /// <returns>
    ///     A new TypeMetadata instance capturing the type's semantic identity.
    /// </returns>
    /// <remarks>
    ///     This is the primary entry point for converting Roslyn's mutable symbol model
    ///     to our immutable metadata representation. Delegates to the extension method
    ///     ToTypeModel for the actual conversion logic.
    /// </remarks>
    public static TypeMetadata FromTypeSymbol(ITypeSymbol typeSymbol) {
        return typeSymbol.ToTypeModel();
    }
}

/// <summary>
///     Provides conversions from Roslyn's symbol model to immutable metadata representations.
/// </summary>
/// <remarks>
///     These extensions bridge the gap between Roslyn's mutable, compilation-bound symbol
///     graph and our immutable, cache-friendly metadata model. The conversion extracts
///     only the essential semantic information needed for code generation.
/// </remarks>
internal static class TypeSymbolExtensions {
    /// <summary>
    ///     Converts a Roslyn type symbol to an immutable TypeMetadata instance.
    /// </summary>
    /// <param name="typeSymbol">
    ///     The type symbol to extract metadata from. May be generic or nested.
    /// </param>
    /// <returns>
    ///     A new TypeMetadata capturing the type's identity, including generic arguments
    ///     and nested type path if applicable.
    /// </returns>
    /// <remarks>
    ///     <para>Nested Type Handling:</para>
    ///     <para>
    ///     For nested types, recursively constructs the full path by walking up the containing
    ///     type chain and concatenating names (e.g., "Outer.Middle.Inner"). This produces a
    ///     flat representation that simplifies equality checking and name generation.
    ///     </para>
    ///     
    ///     <para>Generic Type Processing:</para>
    ///     <para>
    ///     For generic types (INamedTypeSymbol), recursively converts all type arguments.
    ///     Non-generic types receive an empty TypeArguments list. Type parameter constraints
    ///     are not captured since they're not needed for code generation.
    ///     </para>
    ///     
    ///     <para>Location Handling:</para>
    ///     <para>
    ///     Extracts the first location for diagnostic purposes, wrapped in GeneratorIgnored
    ///     to exclude it from equality comparisons and incremental caching decisions.
    ///     </para>
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
    /// <param name="typeSymbol">
    ///     The type symbol representing the target type identity.
    /// </param>
    /// <param name="qualifierMetadata">
    ///     The qualifier metadata (label, custom attribute, or none) that distinguishes
    ///     this binding from others of the same type.
    /// </param>
    /// <returns>
    ///     A new QualifiedTypeMetadata pairing the type with its qualifier.
    /// </returns>
    /// <remarks>
    ///     Qualified types allow the DI framework to distinguish between multiple bindings
    ///     of the same type (e.g., @Named("primary") Database vs @Named("backup") Database).
    ///     This is the standard way to create qualified type references during metadata extraction.
    /// </remarks>
    public static QualifiedTypeMetadata ToQualifiedTypeModel(this ITypeSymbol typeSymbol, IQualifierMetadata qualifierMetadata) {
        return new QualifiedTypeMetadata(typeSymbol.ToTypeModel(), qualifierMetadata);
    }
}
