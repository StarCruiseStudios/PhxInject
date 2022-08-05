﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Specifications.Definitions;

    internal record InjectorProviderDefinition(
            QualifiedTypeModel ProvidedType,
            string InjectorProviderMethodName,
            SpecContainerFactoryInvocationDefinition SpecContainerFactoryInvocation,
            Location Location
    ) : IDefinition;
}