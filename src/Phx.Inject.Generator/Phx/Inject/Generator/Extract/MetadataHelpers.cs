// -----------------------------------------------------------------------------
//  <copyright file="MetadataHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract;

internal static class MetadataHelpers {
    public static IReadOnlyList<QualifiedTypeModel> TryGetMethodParametersQualifiedTypes(
        IMethodSymbol methodSymbol,
        IGeneratorContext generatorCtx) {
        return methodSymbol.Parameters.Select(parameter => {
                var qualifier = GetQualifier(parameter).GetOrThrow(generatorCtx);
                return new QualifiedTypeModel(
                    TypeModel.FromTypeSymbol(parameter.Type),
                    qualifier);
            })
            .ToImmutableList();
    }

    public static IReadOnlyList<QualifiedTypeModel> TryGetConstructorParameterQualifiedTypes(
        ITypeSymbol type,
        IGeneratorContext generatorCtx) {
        var typeLocation = type.Locations.First();

        if (type.IsStatic || type.IsAbstract || type.TypeKind == TypeKind.Interface) {
            return ImmutableList<QualifiedTypeModel>.Empty;
        }

        IReadOnlyList<IMethodSymbol> constructors = type
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.DeclaredAccessibility == Accessibility.Public)
            .ToImmutableList();
        if (constructors.Count != 1) {
            throw Diagnostics.InvalidSpecification.AsException(
                $"Auto injected type '{type.Name}' must contain exactly one public constructor",
                typeLocation,
                generatorCtx);
        }

        var constructorMethod = constructors.Single();

        return TryGetMethodParametersQualifiedTypes(constructorMethod, generatorCtx);
    }

    public static IResult<IQualifier> GetQualifier(this ISymbol symbol) {
        var labelAttributeResult = symbol.TryGetLabelAttribute();
        if (!labelAttributeResult.IsOk) {
            return labelAttributeResult.MapError<IQualifier>();
        }

        var qualifierAttributeResult = symbol.TryGetQualifierAttribute();
        if (!qualifierAttributeResult.IsOk) {
            return qualifierAttributeResult.MapError<IQualifier>();
        }

        var labelAttribute = labelAttributeResult.GetValue();
        var qualifierAttribute = qualifierAttributeResult.GetValue();

        if (labelAttribute != null) {
            if (qualifierAttribute != null) {
                return Result.Error<IQualifier>(
                    $"Symbol {symbol.Name} can only have one Label or Qualifier attribute.",
                    symbol.Locations.First(),
                    Diagnostics.InvalidSpecification);
            }

            return Result.Ok(new LabelQualifier(labelAttribute.Label));
        }

        return qualifierAttribute != null
            ? Result.Ok<IQualifier>(new AttributeQualifier(qualifierAttribute))
            : Result.Ok<IQualifier>(NoQualifier.Instance);
    }

    public static IResult<bool> IsSpecSymbol(ITypeSymbol symbol) {
        return symbol.TryGetSpecificationAttribute()
            .Map(specificationAttribute => {
                if (specificationAttribute == null) {
                    Result.Ok(false);
                }

                var isStaticSpecification = symbol is { TypeKind: TypeKind.Class, IsStatic: true };
                var isInterfaceSpecification = symbol.TypeKind == TypeKind.Interface;

                return isStaticSpecification || isInterfaceSpecification
                    ? Result.Ok(true)
                    : Result.Error<bool>(
                        $"Specification type {symbol.Name} must be a static class or interface.",
                        symbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
            });
    }

    public static IResult<bool> IsDependencySymbol(ITypeSymbol symbol) {
        var isInterface = symbol is { TypeKind: TypeKind.Interface };

        return isInterface
            ? Result.Ok(true)
            : Result.Error<bool>(
                $"Dependency type {symbol.Name} must be an interface.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification);
    }

    public static TypeModel CreateConstructorSpecContainerType(TypeModel injectorType) {
        var specContainerTypeName = NameHelpers.GetAppendedClassName(injectorType, "ConstructorFactories");
        return injectorType with {
            BaseTypeName = specContainerTypeName,
            TypeArguments = ImmutableList<TypeModel>.Empty
        };
    }

    public static IReadOnlyDictionary<string, QualifiedTypeModel> GetRequiredPropertyQualifiedTypes(
        ITypeSymbol type,
        IGeneratorContext generatorCtx) {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod != null && p.SetMethod.DeclaredAccessibility == Accessibility.Public)
            .Where(p => p.IsRequired)
            .ToImmutableDictionary(
                property => property.Name,
                property => new QualifiedTypeModel(
                    TypeModel.FromTypeSymbol(property.Type),
                    GetQualifier(property).GetOrThrow(generatorCtx)
                )
            );
    }

    public static IReadOnlyList<IMethodSymbol> GetDirectBuilderMethods(
        ITypeSymbol type,
        IGeneratorContext generatorCtx) {
        return type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.TryGetBuilderAttribute().GetOrThrow(generatorCtx) != null)
            .ToImmutableList();
    }
}
