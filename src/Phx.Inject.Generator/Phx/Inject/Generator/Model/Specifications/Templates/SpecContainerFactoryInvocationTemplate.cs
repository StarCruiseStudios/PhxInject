// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryInvocationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Templates {
    using Microsoft.CodeAnalysis;
    //
    // internal delegate SpecContainerFactoryInvocationTemplate CreateSpecContainerFactoryMethodInvocationTemplate(
    //     SpecContainerFactoryInvocationDefinition factoryInvocationDefinition,
    //     string specContainerCollectionReferenceName
    // );

    internal record SpecContainerFactoryInvocationTemplate(
            string SpecContainerCollectionReferenceName,
            string SpecContainerReferenceName,
            string SpecContainerFactoryMethodName,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append(
                    $"{SpecContainerCollectionReferenceName}.{SpecContainerReferenceName}.{SpecContainerFactoryMethodName}({SpecContainerCollectionReferenceName})");
        }

        // public class Builder {
        //     public SpecContainerFactoryInvocationTemplate Build(
        //             SpecContainerFactoryInvocationDefinition factoryInvocationDefinition,
        //             string specContainerCollectionReferenceName
        //     ) {
        //         return new SpecContainerFactoryInvocationTemplate(
        //                 specContainerCollectionReferenceName,
        //                 factoryInvocationDefinition.ContainerReference.ReferenceName,
        //                 factoryInvocationDefinition.FactoryMethodName,
        //                 factoryInvocationDefinition.Location
        //         );
        //     }
        // }
    }
}
