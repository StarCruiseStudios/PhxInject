// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactorySingleInvocationDefinition.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;

    internal record SpecContainerFactorySingleInvocationDefinition(
        TypeModel SpecContainerType,
        string FactoryMethodName,
        Location Location
    ) : IDefinition;
}
