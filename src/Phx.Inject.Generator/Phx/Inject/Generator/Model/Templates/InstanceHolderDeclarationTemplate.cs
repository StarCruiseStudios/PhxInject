// -----------------------------------------------------------------------------
//  <copyright file="InstanceHolderDeclarationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate InstanceHolderDeclarationTemplate CreateInstanceHolderDeclarationTemplate(
            SpecContainerFactoryInstanceHolderDefinition instanceHolderDefinition
    );

    internal record InstanceHolderDeclarationTemplate(
            string InstanceQualifiedType,
            string ReferenceName,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"private {InstanceQualifiedType}? {ReferenceName};");
        }

        public class Builder {
            public InstanceHolderDeclarationTemplate Build(
                    SpecContainerFactoryInstanceHolderDefinition instanceHolderDefinition
            ) {
                return new InstanceHolderDeclarationTemplate(
                        instanceHolderDefinition.HeldInstanceType.QualifiedName,
                        instanceHolderDefinition.ReferenceName,
                        instanceHolderDefinition.Location);
            }
        }
    }
}
