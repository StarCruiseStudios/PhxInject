// -----------------------------------------------------------------------------
//  <copyright file="QualifiedTypeModel.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Construct.Definitions;
using Phx.Inject.Generator.Map;

namespace Phx.Inject.Generator.Extract.Model {
    internal record QualifiedTypeModel(TypeModel TypeModel, string Qualifier) {
        public RegistrationIdentifier ToRegistrationIdentifier() {
            return new RegistrationIdentifier(TypeModel.ToTypeDefinition(), Qualifier);
        }
    }
}
