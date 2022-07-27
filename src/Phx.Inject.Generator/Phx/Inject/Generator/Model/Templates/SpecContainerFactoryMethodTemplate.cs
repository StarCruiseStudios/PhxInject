// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal record SpecContainerFactoryMethodTemplate(
            string ReturnTypeQualifiedName,
            string FactoryMethodName,
            string SpecificationQualifiedType,
            string SpecContainerCollectionQualifiedType,
            string SpecContainerCollectionReferenceName,
            string? InstanceHolderReference,
            IEnumerable<SpecContainerFactoryMethodInvocationTemplate> Arguments,
            Location Location
    ) : ISpecContainerMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal {ReturnTypeQualifiedName} {FactoryMethodName}(")
                    .AppendLine($"{SpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}) {{")
                    .IncreaseIndent(1)
                    .Append("return ");
            if (InstanceHolderReference is not null) {
                writer.Append($"{InstanceHolderReference} ??= ");
            }

            writer.Append($"{SpecificationQualifiedType}.{FactoryMethodName}");
            var numArguments = Arguments.Count();
            if (numArguments == 0) {
                writer.AppendLine("();");
            } else {
                writer.AppendLine("(")
                        .IncreaseIndent(1);
                var isFirst = true;
                foreach (var argument in Arguments) {
                    if (!isFirst) {
                        writer.AppendLine(",");
                    }

                    isFirst = false;
                    argument.Render(writer);
                }

                writer.AppendLine(");")
                        .DecreaseIndent(1);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
