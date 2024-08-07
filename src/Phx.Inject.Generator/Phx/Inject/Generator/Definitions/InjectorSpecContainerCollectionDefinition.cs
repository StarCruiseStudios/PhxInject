﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorSpecContainerCollectionDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model;

    // internal delegate SpecContainerCollectionDefinition CreateSpecContainerCollectionDefinition(
    //         InjectorDescriptor injectorDescriptor);

    internal record InjectorSpecContainerCollectionDefinition(
        TypeModel SpecContainerCollectionType,
        IEnumerable<TypeModel> SpecContainers,
        Location Location
    ) : IDefinition {
        // public class Builder {
        //     private readonly CreateSpecContainerCollectionType createSpecContainerCollectionType;
        //     private readonly CreateSpecContainerReferenceDefinition createSpecContainerReference;
        //
        //     public Builder(
        //             CreateSpecContainerCollectionType createSpecContainerCollectionType,
        //             CreateSpecContainerReferenceDefinition createSpecContainerReference
        //     ) {
        //         this.createSpecContainerCollectionType = createSpecContainerCollectionType;
        //         this.createSpecContainerReference = createSpecContainerReference;
        //     }
        //
        //     public SpecContainerCollectionDefinition Build(InjectorDescriptor injectorDescriptor) {
        //         var specContainerCollectionType = createSpecContainerCollectionType(injectorDescriptor.InjectorType);
        //         var specContainerReferences = injectorDescriptor.Specifications.Select(
        //                         specDescriptor => createSpecContainerReference(injectorDescriptor, specDescriptor))
        //                 .ToImmutableList();
        //         return new SpecContainerCollectionDefinition(
        //                 injectorDescriptor.InjectorType,
        //                 specContainerCollectionType,
        //                 specContainerReferences,
        //                 injectorDescriptor.Location);
        //     }
        // }
    }
}
