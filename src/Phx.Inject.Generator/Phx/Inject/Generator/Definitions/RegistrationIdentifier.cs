// -----------------------------------------------------------------------------
//  <copyright file="RegistrationIdentifier.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Model;

namespace Phx.Inject.Generator.Definitions;

internal record RegistrationIdentifier(
    TypeModel RegistrationType,
    string Qualifier
) {
    public static RegistrationIdentifier FromQualifiedTypeModel(QualifiedTypeModel type) {
        return new RegistrationIdentifier(type.TypeModel, type.Qualifier);
    }
}
