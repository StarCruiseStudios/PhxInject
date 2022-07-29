// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerCollectionTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate SpecContainerCollectionTemplate CreateSpecContainerCollectionTemplate(
            SpecContainerCollectionDefinition specContainerCollectionDefinition
    );

    internal record SpecContainerCollectionTemplate(
            string SpecContainerCollectionClassName,
            IEnumerable<SpecContainerCollectionPropertyDefinitionTemplate> SpecContainerProperties,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal record {SpecContainerCollectionClassName} (")
                    .IncreaseIndent(1);
            var isFirst = true;
            foreach (var specContainerProperty in SpecContainerProperties) {
                if (!isFirst) {
                    writer.AppendLine(",");
                    isFirst = false;
                }

                specContainerProperty.Render(writer);
            }

            writer.AppendLine(");")
                    .DecreaseIndent(1);

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            private readonly CreateSpecContainerCollectionPropertyDefinitionTemplate
                    createSpecContainerCollectionPropertyDefinition;

            public Builder(CreateSpecContainerCollectionPropertyDefinitionTemplate createSpecContainerCollectionPropertyDefinition) {
                this.createSpecContainerCollectionPropertyDefinition = createSpecContainerCollectionPropertyDefinition;
            }

            public SpecContainerCollectionTemplate Build(
                    SpecContainerCollectionDefinition specContainerCollectionDefinition
            ) {
                var specContainerProperties = specContainerCollectionDefinition.SpecContainerReferences.Select(
                                specContainerReference => createSpecContainerCollectionPropertyDefinition(specContainerReference))
                        .ToImmutableList();

                return new SpecContainerCollectionTemplate(
                        specContainerCollectionDefinition.SpecContainerCollectionType.TypeName,
                        specContainerProperties,
                        specContainerCollectionDefinition.Location);
            }
        }
    }
}
