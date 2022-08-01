// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTypeGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {

    internal delegate TypeModel CreateSpecContainerType(
            TypeModel injectorType,
            TypeModel specType
    );

    internal static class SpecContainerTypeGenerator {
        public static TypeModel CreateSpecContainerType(TypeModel injectorType, TypeModel specType) {
            return specType with {
                TypeName = $"{injectorType.TypeName}_{specType.TypeName}Container"
            };
        }
    }
}
