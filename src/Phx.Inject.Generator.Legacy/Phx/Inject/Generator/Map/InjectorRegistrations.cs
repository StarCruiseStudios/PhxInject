// -----------------------------------------------------------------------------
// <copyright file="InjectorRegistrations.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Map;

internal record InjectorRegistrations(
    IReadOnlyDictionary<RegistrationIdentifier, List<FactoryRegistration>> FactoryRegistrations,
    IReadOnlyDictionary<RegistrationIdentifier, BuilderRegistration> BuilderRegistrations
);
