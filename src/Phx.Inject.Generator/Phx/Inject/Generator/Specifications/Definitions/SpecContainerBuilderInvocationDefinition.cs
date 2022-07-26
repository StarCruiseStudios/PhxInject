﻿// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderInvocationDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;

    // internal delegate SpecContainerBuilderInvocationDefinition CreateSpecContainerBuilderInvocationDefinition(
    //         InjectorDescriptor injectorDescriptor,
    //         BuilderRegistration builderRegistration,
    //         Location location);

    internal record SpecContainerBuilderInvocationDefinition(
            TypeModel SpecContainerType,
            string BuilderMethodName,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     private readonly CreateSpecContainerReferenceDefinition createSpecContainerReference;
        //
        //     public Builder(CreateSpecContainerReferenceDefinition createSpecContainerReference) {
        //         this.createSpecContainerReference = createSpecContainerReference;
        //     }
        //
        //     public SpecContainerBuilderInvocationDefinition Build(
        //             InjectorDescriptor injectorDescriptor,
        //             BuilderRegistration builderRegistration,
        //             Location location
        //     ) {
        //         return new SpecContainerBuilderInvocationDefinition(
        //                 createSpecContainerReference(injectorDescriptor, builderRegistration.Specification),
        //                 builderRegistration.BuilderDescriptor.BuilderMethodName,
        //                 location);
        //     }
        // }
    }
}
