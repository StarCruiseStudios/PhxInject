// -----------------------------------------------------------------------------
//  <copyright file="InjectorConstructorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal record InjectorConstructorTemplate(
            string InjectorClassName,
            IEnumerable<InjectorConstructorParameterTemplate> Parameters,
            InjectorSpecContainerCollectionInitializationTemplate SpecContainerCollectionInitialization,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"public {InjectorClassName}(");
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
            SpecContainerCollectionInitialization.Render(writer);
            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
