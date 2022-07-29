// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerCollectionPropertyDefinitionTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate SpecContainerCollectionPropertyDefinitionTemplate
            CreateSpecContainerCollectionPropertyDefinitionTemplate(
                    SpecContainerReferenceDefinition specContainerReference
            );

    internal record SpecContainerCollectionPropertyDefinitionTemplate(
            string QualifiedSpecContainerTypeName,
            string PropertyName,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"{QualifiedSpecContainerTypeName} {PropertyName} = new {QualifiedSpecContainerTypeName}()");
        }

        public class Builder {
            public SpecContainerCollectionPropertyDefinitionTemplate
                    Build(SpecContainerReferenceDefinition specContainerReference) {
                return new SpecContainerCollectionPropertyDefinitionTemplate(
                        specContainerReference.SpecContainerType.QualifiedName,
                        specContainerReference.ReferenceName,
                        specContainerReference.Location);
            }
        }
    }
}
