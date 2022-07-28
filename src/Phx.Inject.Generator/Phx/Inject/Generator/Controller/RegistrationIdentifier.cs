// -----------------------------------------------------------------------------
//  <copyright file="RegistrationIdentifier.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Controller {
    using Phx.Inject.Generator.Model;
    using Phx.Inject.Generator.Model.Descriptors;

    internal record RegistrationIdentifier(
            TypeModel RegistrationType,
            string Qualifier
    ) {
        public static RegistrationIdentifier FromQualifiedTypeDescriptor(QualifiedTypeDescriptor type) {
            return new RegistrationIdentifier(type.TypeModel, type.Qualifier);
        }
    }
}
