// -----------------------------------------------------------------------------
//  <copyright file="TypeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using System.Collections.Immutable;

    internal static class TypeHelpers {
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

        public static TypeModel CreateExternalDependencyImplementationType(
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
