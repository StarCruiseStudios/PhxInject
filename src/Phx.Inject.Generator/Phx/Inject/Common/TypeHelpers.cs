// -----------------------------------------------------------------------------
//  <copyright file="TypeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator;

namespace Phx.Inject.Common;

internal static class TypeHelpers {
    public const string FactoryTypeName = "Phx.Inject.Factory";
    public const string InjectionUtilTypeName = "Phx.Inject.InjectionUtil";
    public const string ListTypeName = "System.Collections.Generic.IReadOnlyList";
    public const string HashSetTypeName = "System.Collections.Generic.ISet";
    public const string DictionaryTypeName = "System.Collections.Generic.IReadOnlyDictionary";

    public static readonly IImmutableSet<string> MultiBindTypes = ImmutableHashSet.CreateRange(new[] {
        ListTypeName, HashSetTypeName, DictionaryTypeName
    });

    public static IResult<bool> IsInjectorSymbol(ITypeSymbol symbol) {
        return symbol.TryGetInjectorAttribute()
            .Map(attributeData => {
                if (attributeData == null) {
                    Result.Ok(false);
                }

                var isInterface = symbol is { TypeKind: TypeKind.Interface };

                return isInterface
                    ? Result.Ok(true)
                    : Result.Error<bool>(
                        $"Injector type {symbol.Name} must be an interface.",
                        symbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
            });
    }

    public static IResult<bool> IsSpecSymbol(ITypeSymbol symbol) {
        return symbol.TryGetSpecificationAttribute()
            .Map(attributeData => {
                if (attributeData == null) {
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

    public static bool IsAutoFactoryEligible(QualifiedTypeModel type) {
        var typeSymbol = type.TypeModel.typeSymbol;
        var isVisible = typeSymbol.DeclaredAccessibility == Accessibility.Public
            || typeSymbol.DeclaredAccessibility == Accessibility.Internal;
        return isVisible
            && !typeSymbol.IsStatic
            && !typeSymbol.IsAbstract
            && typeSymbol.TypeKind != TypeKind.Interface
            && type.TypeModel.TypeArguments.Count == 0;
    }

    public static void ValidatePartialType(
        QualifiedTypeModel returnType,
        bool isPartial,
        Location location,
        IGeneratorContext generatorCtx) {
        if (isPartial) {
            if (!MultiBindTypes.Contains(returnType.TypeModel.NamespacedBaseTypeName)) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "Partial factories must return a IReadOnlyList, ISet, or IReadOnlyDictionary.",
                    location,
                    generatorCtx);
            }
        }
    }

    public static string GetQualifiedTypeArgs(QualifiedTypeModel type) {
        return string.Join(
            ", ",
            type.TypeModel.TypeArguments.Select(arg => arg.NamespacedName));
    }

    public static TypeModel CreateSpecContainerType(TypeModel injectorType, TypeModel specType) {
        var specContainerTypeName = NameHelpers.GetCombinedClassName(injectorType, specType);
        return specType with {
            BaseTypeName = specContainerTypeName,
            TypeArguments = ImmutableList<TypeModel>.Empty
        };
    }

    public static TypeModel CreateConstructorSpecContainerType(TypeModel injectorType) {
        var specContainerTypeName = NameHelpers.GetAppendedClassName(injectorType, "ConstructorFactories");
        return injectorType with {
            BaseTypeName = specContainerTypeName,
            TypeArguments = ImmutableList<TypeModel>.Empty
        };
    }

    public static TypeModel CreateSpecContainerCollectionType(TypeModel injectorType) {
        var specContainerCollectionTypeName = injectorType.GetSpecContainerCollectionTypeName();
        return injectorType with {
            BaseTypeName = specContainerCollectionTypeName,
            TypeArguments = ImmutableList<TypeModel>.Empty
        };
    }

    public static TypeModel CreateDependencyImplementationType(
        TypeModel injectorType,
        TypeModel dependencyInterfaceType
    ) {
        var implementationTypeName = NameHelpers.GetCombinedClassName(injectorType, dependencyInterfaceType);
        return injectorType with {
            BaseTypeName = implementationTypeName,
            TypeArguments = ImmutableList<TypeModel>.Empty
        };
    }
}
