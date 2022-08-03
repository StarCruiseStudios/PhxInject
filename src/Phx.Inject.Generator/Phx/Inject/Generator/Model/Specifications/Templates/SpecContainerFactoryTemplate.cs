// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal record SpecContainerFactoryTemplate(
            string ReturnTypeQualifiedName,
            string MethodName,
            string SpecContainerCollectionQualifiedType,
            string SpecContainerCollectionReferenceName,
            string? InstanceHolderReference,
            string? ConstructedSpecificationReference,
            string SpecificationQualifiedType,
            IEnumerable<SpecContainerFactoryInvocationTemplate> Arguments,
            Location Location
    ) : ISpecContainerMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal {ReturnTypeQualifiedName} {MethodName}(")
                    .IncreaseIndent(2)
                    .AppendLine($"{SpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}")
                    .DecreaseIndent(2)
                    .AppendLine(") {")
                    .IncreaseIndent(1)
                    .Append("return ");
            if (!string.IsNullOrEmpty(InstanceHolderReference)) {
                writer.Append($"{InstanceHolderReference} ??= ");
            }

            var referenceName = ConstructedSpecificationReference ?? SpecificationQualifiedType;
            writer.Append($"{referenceName}.{MethodName}");
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
