// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerCollectionImplementationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    using System.Collections.Generic;
    using static Construct.GenerationConstants;

    internal record SpecContainerCollectionImplementationTemplate(
            IEnumerable<SpecContainerPropertyDefinitionTemplate> SpecContainers
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine(
                            $"internal class {SpecContainerCollectionClassName}: {SpecContainerCollectionInterfaceName}  {{")
                    .IncreaseIndent(1);
            foreach (var specContainer in SpecContainers) {
                specContainer.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
