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
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Map;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Common;

internal static class TypeHelpers {
    public static SpecContainerFactoryInvocationDef GetSpecContainerFactoryInvocation(
        InjectorMetadata Injector,
        InjectorRegistrations injectorRegistrations,
        QualifiedTypeModel returnedType,
        Location location,
        IGeneratorContext generatorContext
    ) {
        if (injectorRegistrations.FactoryRegistrations.Count == 0) {
            throw Diagnostics.InternalError.AsFatalException(
                $"Cannot search factory for type {returnedType} before factory registrations are created  while generating injection for type {Injector.InjectorInterfaceType}.",
                location,
                generatorContext);
        }

        TypeModel? runtimeFactoryProvidedType = null;
        var factoryType = returnedType;
        if (returnedType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
            factoryType = returnedType with {
                TypeModel = returnedType.TypeModel.TypeArguments.Single()
            };
            runtimeFactoryProvidedType = factoryType.TypeModel;
        }

        var key = RegistrationIdentifier.FromQualifiedTypeModel(factoryType);
        if (injectorRegistrations.FactoryRegistrations.TryGetValue(key, out var factoryRegistration)) {
            IReadOnlyList<SpecContainerFactorySingleInvocationDef> singleInvocationDefs = factoryRegistration
                .Select(reg => {
                    var specContainerType = CreateSpecContainerType(
                        Injector.InjectorType,
                        reg.Specification.SpecType);
                    return new SpecContainerFactorySingleInvocationDef(
                        specContainerType,
                        reg.FactoryMetadata.GetSpecContainerFactoryName(generatorContext),
                        reg.FactoryMetadata.Location
                    );
                })
                .ToImmutableList();

            return new SpecContainerFactoryInvocationDef(
                singleInvocationDefs,
                factoryType,
                runtimeFactoryProvidedType,
                location);
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find factory for type {factoryType} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            generatorContext);
    }

    public static bool IsAutoFactoryEligible(QualifiedTypeModel type) {
        var typeSymbol = type.TypeModel.TypeSymbol;
        var isVisible = typeSymbol.DeclaredAccessibility == Accessibility.Public
            || typeSymbol.DeclaredAccessibility == Accessibility.Internal;
        return isVisible
            && !typeSymbol.IsStatic
            && !typeSymbol.IsAbstract
            && typeSymbol.TypeKind != TypeKind.Interface
            && type.TypeModel.TypeArguments.Count == 0;
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
