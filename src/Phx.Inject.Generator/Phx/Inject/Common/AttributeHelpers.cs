// -----------------------------------------------------------------------------
//  <copyright file="AttributeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Common;

internal static class AttributeHelpers {
    public const string PhxInjectNamespace = "Phx.Inject";
    public const string InjectorAttributeShortName = "Injector";
    public const string InjectorAttributeBaseName = nameof(InjectorAttribute);
    public const string PhxInjectAttributeShortName = "PhxInject";
    public const string PhxInjectAttributeBaseName = nameof(PhxInjectAttribute);
    public const string SpecificationAttributeShortName = "Specification";
    public const string SpecificationAttributeBaseName = nameof(SpecificationAttribute);

    public const string BuilderAttributeClassName = $"{PhxInjectNamespace}.{nameof(BuilderAttribute)}";

    public const string BuilderReferenceAttributeClassName =
        $"{PhxInjectNamespace}.{nameof(BuilderReferenceAttribute)}";

    public const string ChildInjectorAttributeClassName = $"{PhxInjectNamespace}.{nameof(ChildInjectorAttribute)}";
    public const string DependencyAttributeClassName = $"{PhxInjectNamespace}.{nameof(DependencyAttribute)}";
    public const string FactoryAttributeClassName = $"{PhxInjectNamespace}.{nameof(FactoryAttribute)}";

    public const string FactoryReferenceAttributeClassName =
        $"{PhxInjectNamespace}.{nameof(FactoryReferenceAttribute)}";

    public const string InjectorAttributeClassName = $"{PhxInjectNamespace}.{nameof(InjectorAttribute)}";
    public const string LabelAttributeClassName = $"{PhxInjectNamespace}.{nameof(LabelAttribute)}";
    public const string LinkAttributeClassName = $"{PhxInjectNamespace}.{nameof(LinkAttribute)}";
    public const string PartialAttributeClassName = $"{PhxInjectNamespace}.{nameof(PartialAttribute)}";
    public const string PhxInjectAttributeClassName = $"{PhxInjectNamespace}.{nameof(PhxInjectAttribute)}";
    public const string QualifierAttributeClassName = $"{PhxInjectNamespace}.{nameof(QualifierAttribute)}";
    public const string SpecificationAttributeClassName = $"{PhxInjectNamespace}.{nameof(SpecificationAttribute)}";

    public static IResult<AttributeData?> TryGetPhxInjectAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, PhxInjectAttributeClassName);
    }

    public static IResult<AttributeData> ExpectPhxInjectAttribute(this ISymbol symbol) {
        return ExpectSingleAttribute(symbol, PhxInjectAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetInjectorAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, InjectorAttributeClassName);
    }

    public static IResult<AttributeData> ExpectInjectorAttribute(this ISymbol symbol) {
        return ExpectSingleAttribute(symbol, InjectorAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetSpecificationAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, SpecificationAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetDependencyAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, DependencyAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetLabelAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, LabelAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetQualifierAttribute(this ISymbol symbol) {
        return TryGetSingleAttributedAttribute(symbol, QualifierAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetAllLinkAttributes(this ISymbol symbol) {
        return TryGetAttributes(symbol, LinkAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetFactoryAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, FactoryAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetFactoryReferenceAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, FactoryReferenceAttributeClassName);
    }

    public static IResult<bool> HasBuilderAttribute(this ISymbol symbol) {
        return TryGetBuilderAttribute(symbol).Map(it => Result.Ok(it is not null));
    }

    public static IResult<AttributeData?> TryGetBuilderAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, BuilderAttributeClassName);
    }

    public static IResult<bool> HasBuilderReferenceAttribute(this ISymbol symbol) {
        return TryGetBuilderReferenceAttribute(symbol).Map(it => Result.Ok(it is not null));
    }

    public static IResult<AttributeData?> TryGetBuilderReferenceAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, BuilderReferenceAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetChildInjectorAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, ChildInjectorAttributeClassName);
    }

    public static IResult<AttributeData?> TryGetPartialAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, PartialAttributeClassName);
    }

    private static IReadOnlyList<AttributeData> TryGetAttributes(ISymbol symbol, string attributeClassName) {
        return symbol.GetAttributes()
            .Where(attributeData => attributeData.AttributeClass!.ToString() == attributeClassName)
            .ToImmutableList();
    }

    private static IResult<IReadOnlyList<AttributeData>> ExpectAttributes(ISymbol symbol, string attributeClassName) {
        var result = TryGetAttributes(symbol, attributeClassName);
        return result.Any()
            ? Result.Ok(result)
            : Result.Error<IReadOnlyList<AttributeData>>(
                $"Expected type {symbol.Name} to have at least one {attributeClassName}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification);
    }

    private static IResult<AttributeData?> TryGetSingleAttribute(ISymbol symbol, string attributeClassName) {
        var attributes = TryGetAttributes(symbol, attributeClassName);
        return attributes.Count switch {
            > 1 => Result.Error<AttributeData?>(
                $"Type {symbol.Name} can only have one {attributeClassName}. Found {attributes.Count}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification),
            _ => Result.Ok<AttributeData?>(attributes.SingleOrDefault())
        };
    }

    private static IResult<AttributeData> ExpectSingleAttribute(ISymbol symbol, string attributeClassName) {
        return TryGetSingleAttribute(symbol, attributeClassName)
            .Map(attributeData => Result.FromNullable(
                attributeData,
                $"Expected type {symbol.Name} to have one {attributeClassName}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification));
    }

    private static IReadOnlyList<AttributeData> TryGetAttributedAttributes(
        ISymbol symbol,
        string parentAttributeClassName) {
        return symbol.GetAttributes()
            .Where(attributeData => TryGetAttributes(attributeData.AttributeClass!, parentAttributeClassName).Any())
            .ToImmutableList();
    }

    private static IResult<AttributeData?> TryGetSingleAttributedAttribute(
        ISymbol symbol,
        string parentAttributeClassName) {
        var attributes = TryGetAttributedAttributes(symbol, parentAttributeClassName);
        return attributes.Count switch {
            > 1 => Result.Error<AttributeData?>(
                $"Type {symbol.Name} can only have one {parentAttributeClassName}. Found {attributes.Count}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification),
            _ => Result.Ok<AttributeData?>(attributes.SingleOrDefault())
        };
    }
}
