// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record SpecContainerTemplate(
            string SpecContainerClassName,
            string? ConstructedSpecClassQualifiedName,
            string ConstructedSpecificationReferenceName,
            IEnumerable<InstanceHolderDeclarationTemplate> InstanceHolderDeclarationTemplates,
            IEnumerable<ISpecContainerMemberTemplate> MemberTemplates,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal class {SpecContainerClassName} {{")
                    .IncreaseIndent(1);
            
            foreach (var instanceHolderDeclarationTemplate in InstanceHolderDeclarationTemplates) {
                instanceHolderDeclarationTemplate.Render(writer);
            }

            if (ConstructedSpecClassQualifiedName != null) {
                writer.AppendLine(
                                $"private {ConstructedSpecClassQualifiedName} {ConstructedSpecificationReferenceName};")
                        .AppendBlankLine()
                        .AppendLine(
                                $"public {SpecContainerClassName}({ConstructedSpecClassQualifiedName} {ConstructedSpecificationReferenceName}) {{")
                        .IncreaseIndent(1)
                        .AppendLine(
                                $"this.{ConstructedSpecificationReferenceName} = {ConstructedSpecificationReferenceName};")
                        .DecreaseIndent(1)
                        .AppendLine("}");
            }

            foreach (var memberTemplate in MemberTemplates) {
                writer.AppendBlankLine();
                memberTemplate.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
