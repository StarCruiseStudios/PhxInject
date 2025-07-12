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
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Generator.Extract;

internal static class MetadataHelpers {
    public static IReadOnlyList<QualifiedTypeModel> TryGetMethodParametersQualifiedTypes(
        IMethodSymbol methodSymbol,
        IGeneratorContext generatorCtx) {
        return methodSymbol.Parameters.Select(parameter => {
                var qualifier = QualifierMetadata.Extractor.Instance.Extract(parameter, generatorCtx);
                return new QualifiedTypeModel(
                    TypeModel.FromTypeSymbol(parameter.Type),
                    qualifier);
            })
            .ToImmutableList();
    }

    public static IReadOnlyList<QualifiedTypeModel> TryGetConstructorParameterQualifiedTypes(
        ITypeSymbol type,
        IGeneratorContext generatorCtx) {
        var typeLocation = type.GetLocationOrDefault();

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
                    QualifierMetadata.Extractor.Instance.Extract(property, generatorCtx)
                )
            );
    }
}
