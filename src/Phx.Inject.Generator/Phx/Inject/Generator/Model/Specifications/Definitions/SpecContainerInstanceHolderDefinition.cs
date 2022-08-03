// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerInstanceHolderDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Definitions {
    using Microsoft.CodeAnalysis;

    // internal delegate SpecContainerInstanceHolderDefinition CreateSpecContainerInstanceHolderDefinition(
    //         SpecFactoryDescriptor factoryDescriptor,
    //         IDefinitionGenerationContext context
    // );

    internal record SpecContainerInstanceHolderDefinition(
            QualifiedTypeModel HeldInstanceType,
            SpecFactoryMethodFabricationMode FabricationMode,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     public SpecContainerInstanceHolderDefinition Build(
        //             SpecFactoryDescriptor factoryDescriptor,
        //             IDefinitionGenerationContext context
        //     ) {
        //         return new SpecContainerInstanceHolderDefinition(
        //                 factoryDescriptor.ReturnType,
        //                 factoryDescriptor.FabricationMode,
        //                 factoryDescriptor.Location);
        //     }
        // }
    }
}
