// -----------------------------------------------------------------------------
//  <copyright file="InstanceHolderNameGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Text.RegularExpressions;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate string CreateInstanceHolderName(QualifiedTypeDescriptor heldInstanceType);

    internal static class InstanceHolderNameGenerator {
        private static Regex validCharsRegex = new Regex(@"[^a-zA-Z0-9_]");

        public static string CreateInstanceHolderName(QualifiedTypeDescriptor heldInstanceType) {
            string referenceName = string.IsNullOrEmpty(heldInstanceType.Qualifier)
                    ? heldInstanceType.TypeModel.TypeName
                    : $"{heldInstanceType.Qualifier}_{heldInstanceType.TypeModel.TypeName}";
            referenceName = validCharsRegex.Replace(referenceName, "");
            referenceName = char.ToLower(referenceName[0]) + referenceName[1..];
            return referenceName;
        }
    }
}
