// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryRequiredPropertyDefinition.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Definitions;

internal record SpecContainerFactoryRequiredPropertyDefinition(
    string PropertyName,
    SpecContainerFactoryInvocationDefinition Value,
    Location Location
) : IDefinition;
