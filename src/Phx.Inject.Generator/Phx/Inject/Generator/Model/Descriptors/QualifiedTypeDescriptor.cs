// -----------------------------------------------------------------------------
//  <copyright file="QualifiedTypeDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using Microsoft.CodeAnalysis;

    internal record QualifiedTypeDescriptor(
            TypeModel TypeModel,
            string Qualifier,
            Location Location
    ) : IDescriptor {
        public override string ToString() {
            return string.IsNullOrEmpty(Qualifier)
                    ? TypeModel.ToString()
                    : $"[{Qualifier}] {TypeModel}";
        }

        public const string NoQualifier = "";
    }
}
