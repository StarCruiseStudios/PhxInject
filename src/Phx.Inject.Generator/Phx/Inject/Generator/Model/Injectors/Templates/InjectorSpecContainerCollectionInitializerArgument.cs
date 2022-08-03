// -----------------------------------------------------------------------------
//  <copyright file="InjectorSpecContainerCollectionInitializerArgument.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal record InjectorSpecContainerCollectionInitializerArgument(
            string ParameterName,
            string SpecContainerTypeQualifiedName,
            IEnumerable<string> Arguments,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"{ParameterName}: new {SpecContainerTypeQualifiedName}(");
            if (Arguments.Count() > 0) {
                writer.AppendLine()
                        .IncreaseIndent(2);
                var isFirst = true;
                foreach (var argument in Arguments) {
                    if (isFirst) {
                        isFirst = false;
                    } else {
                        writer.AppendLine(",");
                    }

                    writer.Append(argument);
                }

                writer.DecreaseIndent(2);

            }

            writer.Append(")");
        }
    }
}
