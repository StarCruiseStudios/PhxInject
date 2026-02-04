// -----------------------------------------------------------------------------
// <copyright file="IQualifier.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common.Model;

internal interface IQualifier {
    string Identifier { get; }
}

internal class LabelQualifier : IQualifier {
    private readonly string baseIdentifier;

    public LabelQualifier(string qualifier) {
        baseIdentifier = qualifier;
        Identifier = "L_" + qualifier;
    }
    public string Identifier { get; }

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
    public static readonly NoQualifier Instance = new();

    private NoQualifier() { }

    public string Identifier {
        get => "";
    }

    public override bool Equals(object? obj) {
        return obj is NoQualifier;
    }

    public override int GetHashCode() {
        return typeof(NoQualifier).GetHashCode();
    }
}

internal class CustomQualifier : IQualifier {
    public ITypeSymbol AttributeType { get; }

    public CustomQualifier(ITypeSymbol attributeType) {
        AttributeType = attributeType;
        Identifier = "A_" + attributeType;
    }
    public string Identifier { get; }

    public override string ToString() {
        return $"@{AttributeType.Name}";
    }

    public override bool Equals(object? obj) {
        return obj is CustomQualifier qualifier
            && SymbolEqualityComparer.IncludeNullability.Equals(
                AttributeType,
                qualifier.AttributeType);
    }

    public override int GetHashCode() {
        return (typeof(CustomQualifier).GetHashCode() * 397)
            ^ SymbolEqualityComparer.IncludeNullability.GetHashCode(AttributeType);
    }
}
