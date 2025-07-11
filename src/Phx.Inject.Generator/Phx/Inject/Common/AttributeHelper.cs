// -----------------------------------------------------------------------------
// <copyright file="AttributeHelper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Common;

internal interface IAttributeHelper {
    bool HasAttribute(ISymbol symbol, string attributeClassName);
    IReadOnlyList<IResult<T>> GetAttributes<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create);
    IResult<T> ExpectSingleAttribute<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create
    );
}

internal class AttributeHelper : IAttributeHelper {
    public static IAttributeHelper Instance { get; } = new AttributeHelper();
    public bool HasAttribute(ISymbol symbol, string attributeClassName) {
        return symbol.GetAttributes()
            .Any(attributeData => attributeData.AttributeClass?.ToString() == attributeClassName);
    }

    public IReadOnlyList<IResult<T>> GetAttributes<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create) {
        return symbol.GetAttributes()
            .Where(attributeData => attributeData.AttributeClass?.ToString() == attributeClassName)
            .Select(create)
            .ToImmutableList();
    }

    public IResult<T> ExpectSingleAttribute<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create
    ) {
        var attributes = GetAttributes(symbol, attributeClassName, create);
        return attributes.Count switch {
            1 => attributes.Single(),
            > 1 => Result.Error<T>(
                $"Type {symbol.Name} cannot have more than one {attributeClassName}. Found {attributes.Count}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification),
            _ => Result.Error<T>(
                $"Type {symbol.Name} must have an {attributeClassName}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification)
        };
    }

    public IResult<T?> TryGetSingleAttribute<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T?>> create
    ) where T : class? {
        var attributes = GetAttributes(symbol, attributeClassName, create);
        return attributes.Count switch {
            1 => attributes.Single(),
            > 1 => Result.Error<T?>(
                $"Type {symbol.Name} can only have one {attributeClassName}. Found {attributes.Count}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification),
            _ => Result.Ok<T?>(null)
        };
    }
}
