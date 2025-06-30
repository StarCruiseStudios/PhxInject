// -----------------------------------------------------------------------------
//  <copyright file="TypeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model;

    internal static class TypeHelpers {
        public const string FactoryTypeName = "Phx.Inject.Factory";
        public const string InjectionUtilTypeName = "Phx.Inject.InjectionUtil";
        public const string ListTypeName = "System.Collections.Generic.List";
        public const string HashSetTypeName = "System.Collections.Generic.HashSet";
        public const string DictionaryTypeName = "System.Collections.Generic.Dictionary";

        public static readonly IImmutableSet<string> MultiBindTypes = ImmutableHashSet.CreateRange(new[]{
            ListTypeName,
            HashSetTypeName,
            DictionaryTypeName
        });

        public static void ValidatePartialType(QualifiedTypeModel returnType, bool isPartial, Location location) {
            if (isPartial) {
                if (!MultiBindTypes.Contains(returnType.TypeModel.QualifiedBaseTypeName)) {
                    throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        "Partial factories must return a List, HashSet, or Dictionary.",
                        location);
                }
            }
        }

        public static string GetQualifiedTypeArgs(QualifiedTypeModel type) {
            return string.Join(
                ", ",
                type.TypeModel.TypeArguments.Select(arg => arg.QualifiedName));
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
}
