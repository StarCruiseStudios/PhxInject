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
using Phx.Inject.Generator;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Common;

internal interface IAttributeHelper {
    bool HasAttribute(ISymbol symbol, string attributeClassName);
    IReadOnlyList<IResult<T>> GetAttributesResult<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create);
    IResult<T> ExpectSingleAttributeResult<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create
    );
    IReadOnlyList<T> GetAttributes<T>(
        ISymbol symbol,
        string attributeClassName,
        IGeneratorContext generatorCtx,
        Func<AttributeData, T> create);
    T ExpectSingleAttribute<T>(
        ISymbol symbol,
        string attributeClassName,
        IGeneratorContext generatorCtx,
        Func<AttributeData, T> create
    );
}

internal class AttributeHelper : IAttributeHelper {
    public static IAttributeHelper Instance { get; } = new AttributeHelper();
    public bool HasAttribute(ISymbol symbol, string attributeClassName) {
        return symbol.GetAttributes()
            .Any(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName);
    }

    public IReadOnlyList<IResult<T>> GetAttributesResult<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create) {
        return symbol.GetAttributes()
            .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
            .Select(create)
            .ToImmutableList();
    }

    public IResult<T> ExpectSingleAttributeResult<T>(
        ISymbol symbol,
        string attributeClassName,
        Func<AttributeData, IResult<T>> create
    ) {
        var attributes = GetAttributesResult(symbol, attributeClassName, create);
        return attributes.Count switch {
            1 => attributes.Single(),
            > 1 => Result.Error<T>(
                $"Symbol {symbol.Name} cannot have more than one {attributeClassName}. Found {attributes.Count}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification),
            _ => Result.Error<T>(
                $"Symbol {symbol.Name} must have an {attributeClassName}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification)
        };
    }

    public IReadOnlyList<T> GetAttributes<T>(
        ISymbol symbol,
        string attributeClassName,
        IGeneratorContext generatorCtx,
        Func<AttributeData, T> create) {
        return symbol.GetAttributes()
            .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
            .SelectCatching(
                generatorCtx.Aggregator,
                attributeData => $"extracting attribute ${attributeData.GetFullyQualifiedName()}",
                create)
            .ToImmutableList();
    }

    public T ExpectSingleAttribute<T>(
        ISymbol symbol,
        string attributeClassName,
        IGeneratorContext generatorCtx,
        Func<AttributeData, T> create
    ) {
        var attributes = symbol.GetAttributes()
            .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
            .ToImmutableList();

        return attributes.Count switch {
            1 => create(attributes.Single()),
            > 1 => throw Diagnostics.InvalidSpecification.AsException(
                $"Type {symbol.Name} cannot have more than one {attributeClassName}. Found {attributes.Count}.",
                symbol.Locations.First(),
                generatorCtx),
            _ => throw Diagnostics.InvalidSpecification.AsException(
                $"Type {symbol.Name} must have an {attributeClassName}.",
                symbol.Locations.First(),
                generatorCtx)
        };
    }
}
