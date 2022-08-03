// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderInvocationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Templates {
    using Microsoft.CodeAnalysis;

    // internal delegate SpecContainerBuilderInvocationTemplate
    //         CreateSpecContainerBuilderMethodInvocationTemplate(
    //                 SpecContainerBuilderInvocationDefinition specContainerBuilderInvocationDefinition,
    //                 string specContainerCollectionReferenceName,
    //                 string builderTargetName
    //         );

    internal record SpecContainerBuilderInvocationTemplate(
            string SpecContainerCollectionReferenceName,
            string SpecContainerReferenceName,
            string SpecContainerBuilderMethodName,
            string BuilderTargetReferenceName,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append(
                    $"{SpecContainerCollectionReferenceName}.{SpecContainerReferenceName}.{SpecContainerBuilderMethodName}({BuilderTargetReferenceName}, {SpecContainerCollectionReferenceName})");
        }

        // public class Builder {
        //     public SpecContainerBuilderInvocationTemplate Build(
        //             SpecContainerBuilderInvocationDefinition specContainerBuilderInvocationDefinition,
        //             string specContainerCollectionReferenceName,
        //             string builderTargetName
        //     ) {
        //         return new SpecContainerBuilderInvocationTemplate(
        //                 specContainerCollectionReferenceName,
        //                 specContainerBuilderInvocationDefinition.ContainerReference.ReferenceName,
        //                 specContainerBuilderInvocationDefinition.BuilderMethodName,
        //                 builderTargetName,
        //                 specContainerBuilderInvocationDefinition.Location
        //         );
        //     }
        // }
    }
}
