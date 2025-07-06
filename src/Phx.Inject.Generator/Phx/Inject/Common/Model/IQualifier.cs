// -----------------------------------------------------------------------------
// <copyright file="IQualifier.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Common.Model;

internal interface IQualifier {
    string Identifier { get; }
}

internal class LabelQualifier : IQualifier {
    private readonly string baseIdentifier;
    public string Identifier { get; }

    public LabelQualifier(string qualifier) {
        baseIdentifier = qualifier;
        Identifier = "L_" + qualifier;
    }

    public override string ToString() {
        return $"\"{baseIdentifier}\"";
    }

    public override bool Equals(object? obj) {
        return obj is LabelQualifier qualifier
            && baseIdentifier.Equals(qualifier.baseIdentifier);
    }

    public override int GetHashCode() {
        return (typeof(LabelQualifier).GetHashCode() * 397) ^ Identifier.GetHashCode();
    }
}

internal class NoQualifier : IQualifier {
    public string Identifier => "";
    
    private NoQualifier() { }
    public static readonly NoQualifier Instance = new();
    
    public override bool Equals(object? obj) {
        return obj is NoQualifier;
    }

    public override int GetHashCode() {
        return typeof(NoQualifier).GetHashCode();
    }
}

internal class AttributeQualifier : IQualifier {
    public QualifierAttributeDesc Attribute { get; }
    public string Identifier { get; }

    public AttributeQualifier(QualifierAttributeDesc attribute) {
        Attribute = attribute;
        Identifier = "A_" + attribute.AttributeTypeSymbol;
    }

    public override string ToString() {
        return $"@{Attribute.AttributeTypeSymbol.Name}";
    }

    public override bool Equals(object? obj) {
        return obj is AttributeQualifier qualifier
            && SymbolEqualityComparer.IncludeNullability.Equals(
                Attribute.AttributeTypeSymbol,
                qualifier.Attribute.AttributeTypeSymbol);
    }

    public override int GetHashCode() {
        return (typeof(AttributeQualifier).GetHashCode() * 397) 
            ^ SymbolEqualityComparer.IncludeNullability.GetHashCode(Attribute.AttributeTypeSymbol);
    }
}