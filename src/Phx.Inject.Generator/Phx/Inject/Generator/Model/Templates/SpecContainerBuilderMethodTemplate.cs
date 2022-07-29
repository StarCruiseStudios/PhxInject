// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate SpecContainerBuilderMethodTemplate CreateSpecContainerBuilderMethodTemplate(
            SpecContainerBuilderMethodDefinition specContainerBuilderMethodDefinition
    );

    internal record SpecContainerBuilderMethodTemplate(
            string BuiltTypeQualifiedName,
            string BuilderMethodName,
            string BuiltInstanceReferenceName,
            string SpecificationQualifiedType,
            string SpecContainerCollectionQualifiedType,
            string SpecContainerCollectionReferenceName,
            IEnumerable<SpecContainerFactoryMethodInvocationTemplate> Arguments,
            Location Location
    ) : ISpecContainerMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal void {BuilderMethodName}(")
                    .AppendLine($"{BuiltTypeQualifiedName} {BuiltInstanceReferenceName}, {SpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}) {{")
                    .IncreaseIndent(1);

            writer.Append($"{SpecificationQualifiedType}.{BuilderMethodName}(");
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
