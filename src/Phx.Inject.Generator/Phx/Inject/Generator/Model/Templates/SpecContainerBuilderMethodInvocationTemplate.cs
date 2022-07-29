// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderMethodInvocationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate SpecContainerBuilderMethodInvocationTemplate
            CreateSpecContainerBuilderMethodInvocationTemplate(
                    SpecContainerBuilderInvocationDefinition specContainerBuilderInvocationDefinition,
                    string specContainerCollectionReferenceName,
                    string builderTargetName
            );

    internal record SpecContainerBuilderMethodInvocationTemplate(
            string SpecContainersReferenceName,
            string ContainerReferenceName,
            string SpecContainerBuilderMethodName,
            string BuilderTargetReferenceName,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append(
                    $"{SpecContainersReferenceName}.{ContainerReferenceName}.{SpecContainerBuilderMethodName}({BuilderTargetReferenceName}, {SpecContainersReferenceName})");
        }

        public class Builder {
            public SpecContainerBuilderMethodInvocationTemplate Build(
                    SpecContainerBuilderInvocationDefinition specContainerBuilderInvocationDefinition,
                    string specContainerCollectionReferenceName,
                    string builderTargetName
            ) {
                return new SpecContainerBuilderMethodInvocationTemplate(
                        specContainerCollectionReferenceName,
                        specContainerBuilderInvocationDefinition.ContainerReference.ReferenceName,
                        specContainerBuilderInvocationDefinition.BuilderMethodName,
                        builderTargetName,
                        specContainerBuilderInvocationDefinition.Location
                );
            }
        }
    }
}
