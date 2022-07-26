// -----------------------------------------------------------------------------
//  <copyright file="RegistrationIdentifier.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Construct.Definitions;

namespace Phx.Inject.Generator.Map {
    internal record RegistrationIdentifier(
        TypeDefinition TypeDefinition,
        string Qualifier
    ) {
        public const string DefaultQualifier = "";
    };
}
