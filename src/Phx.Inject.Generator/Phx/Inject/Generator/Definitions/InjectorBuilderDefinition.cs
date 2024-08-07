﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model;

    internal record InjectorBuilderDefinition(
        QualifiedTypeModel BuiltType,
        string InjectorBuilderMethodName,
        SpecContainerBuilderInvocationDefinition SpecContainerBuilderInvocation,
        Location Location
    ) : IDefinition;
}