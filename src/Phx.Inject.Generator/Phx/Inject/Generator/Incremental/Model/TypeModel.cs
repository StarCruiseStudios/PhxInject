using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Incremental.Model;

internal record TypeModel(
    string NamespaceName,
    string BaseTypeName,
    IReadOnlyList<TypeModel> TypeArguments,
    SourceLocation Location
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
    
    public virtual bool Equals(TypeModel? other) {
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
            new SourceLocation(typeSymbol.Locations.FirstOrDefault() ?? Location.None));
    }
}