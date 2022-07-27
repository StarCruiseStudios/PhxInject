// -----------------------------------------------------------------------------
//  <copyright file="InjectorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record InjectorTemplate(
            string InjectorClassName,
            string InjectorInterfaceQualifiedName,
            SpecContainerCollectionTemplate SpecContainerCollectionTemplate,
            string SpecContainerCollectionClassName,
            string SpecContainersReferenceName,
            IEnumerable<IInjectorMemberTemplate> InjectorMemberTemplates,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal partial class {InjectorClassName} : {InjectorInterfaceQualifiedName} {{")
                    .IncreaseIndent(1);
            SpecContainerCollectionTemplate.Render(writer);

            writer.AppendBlankLine()
                    .AppendLine($"private readonly {SpecContainerCollectionClassName} {SpecContainersReferenceName} = new {SpecContainerCollectionClassName}();");

            foreach (var memberTemplate in InjectorMemberTemplates) {
                writer.AppendBlankLine();
                memberTemplate.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
