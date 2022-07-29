// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryMethodInvocationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate SpecContainerFactoryMethodInvocationTemplate CreateSpecContainerFactoryMethodInvocationTemplate(
        SpecContainerFactoryInvocationDefinition factoryInvocationDefinition,
        string specContainerCollectionReferenceName
    );

    internal record SpecContainerFactoryMethodInvocationTemplate(
            string SpecContainerCollectionReferenceName,
            string ContainerReferenceName,
            string SpecContainerFactoryMethodName,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append(
                    $"{SpecContainerCollectionReferenceName}.{ContainerReferenceName}.{SpecContainerFactoryMethodName}({SpecContainerCollectionReferenceName})");
        }

        public class Builder {
            public SpecContainerFactoryMethodInvocationTemplate Build(
                    SpecContainerFactoryInvocationDefinition factoryInvocationDefinition,
                    string specContainerCollectionReferenceName
            ) {
                return new SpecContainerFactoryMethodInvocationTemplate(
                        specContainerCollectionReferenceName,
                        factoryInvocationDefinition.ContainerReference.ReferenceName,
                        factoryInvocationDefinition.FactoryMethodName,
                        factoryInvocationDefinition.Location
                );
            }
        }
    }
}
