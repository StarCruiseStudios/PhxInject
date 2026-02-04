// -----------------------------------------------------------------------------
// <copyright file="FactoryRegistration.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Generator.Map;

internal record FactoryRegistration(
    SpecMetadata Specification,
    SpecFactoryMetadata FactoryMetadata
);
