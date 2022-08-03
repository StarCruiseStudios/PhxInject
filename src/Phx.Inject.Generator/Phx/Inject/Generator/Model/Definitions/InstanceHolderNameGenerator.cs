// -----------------------------------------------------------------------------
//  <copyright file="InstanceHolderNameGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using Phx.Inject.Generator.Input;

    internal delegate string CreateInstanceHolderName(QualifiedTypeModel heldInstanceType);

    internal static class InstanceHolderNameGenerator {

        public static string CreateInstanceHolderName(QualifiedTypeModel heldInstanceType) {
            string referenceName = string.IsNullOrEmpty(heldInstanceType.Qualifier)
                    ? heldInstanceType.TypeModel.TypeName
                    : $"{heldInstanceType.Qualifier}_{heldInstanceType.TypeModel.TypeName}";
            referenceName = SymbolProcessors.GetValidReferenceName(referenceName, startLowercase: true);
            return referenceName;
        }
    }
}
