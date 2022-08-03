// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerConstructorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal record SpecContainerConstructorTemplate(
            string SpecContainerClassName,
            IEnumerable<SpecContainerConstructorParameterTemplate> Parameters,
            IEnumerable<SpecContainerConstructorAssignmentTemplate> Assignments,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"public {SpecContainerClassName}(");
            if (Parameters.Any()) {
                writer.AppendLine().IncreaseIndent(2);

                var isFirst = true;
                foreach (var parameter in Parameters) {
                    if (isFirst) {
                        isFirst = false;
                    } else {
                        writer.AppendLine(",");
                    }

                    parameter.Render(writer);
                }

                writer.DecreaseIndent(2)
                        .AppendLine();
            }

            writer.AppendLine(") {")
                    .IncreaseIndent(1);
            
            foreach (var assignment in Assignments) {
                assignment.Render(writer);
            }
            
            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
