﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderMethodDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate InjectorBuilderMethodDefinition CreateInjectorBuilderMethodDefinition(
            InjectorBuilderMethodDescriptor injectorBuilderMethodDescriptor,
            InjectorDescriptor injectorDescriptor,
            IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
    );

    internal record InjectorBuilderMethodDefinition(
            TypeModel BuiltType,
            string InjectorMethodName,
            SpecContainerBuilderMethodDefinition SpecContainerBuilderMethod,
            Location Location) : IDefinition;
}
