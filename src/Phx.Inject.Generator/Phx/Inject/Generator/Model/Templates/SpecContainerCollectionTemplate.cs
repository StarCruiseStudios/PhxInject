// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerCollectionTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record SpecContainerCollectionTemplate(
            string SpecContainerCollectionClassName,
            IEnumerable<SpecContainerCollectionPropertyDefinitionTemplate> SpecContainersProperties,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine(
                            $"internal record {SpecContainerCollectionClassName} (")
                    .IncreaseIndent(1);
            var isFirst = true;
            foreach (var SpecContainersProperty in SpecContainersProperties) {
                if (!isFirst) {
                    writer.AppendLine(",");
                    isFirst = false;
                }
                SpecContainersProperty.Render(writer);
            }

            writer.AppendLine(");")
                    .DecreaseIndent(1);

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
