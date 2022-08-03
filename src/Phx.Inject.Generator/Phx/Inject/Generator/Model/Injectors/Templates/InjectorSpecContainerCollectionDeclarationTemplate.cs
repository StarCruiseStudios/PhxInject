// -----------------------------------------------------------------------------
//  <copyright file="InjectorSpecContainerCollectionDeclarationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record InjectorSpecContainerCollectionDeclarationTemplate(
            string SpecContainerCollectionClassName,
            IEnumerable<InjectorSpecContainerCollectionPropertyDeclarationTemplate> PropertyDeclarations,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal record {SpecContainerCollectionClassName} (")
                    .IncreaseIndent(2);

            var isFirst = true;
            foreach (var propertyDeclaration in PropertyDeclarations) {
                if (isFirst) {
                    isFirst = false;
                } else {
                    writer.AppendLine(",");
                }

                propertyDeclaration.Render(writer);
            }

            writer.AppendLine(");")
                    .DecreaseIndent(2);
        }
    }
}
