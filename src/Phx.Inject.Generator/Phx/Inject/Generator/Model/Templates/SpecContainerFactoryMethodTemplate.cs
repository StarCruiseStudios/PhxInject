// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate SpecContainerFactoryMethodTemplate CreateSpecContainerFactoryMethodTemplate(
            SpecContainerFactoryMethodDefinition specContainerFactoryMethodDefinition
    );

    internal record SpecContainerFactoryMethodTemplate(
            string ReturnTypeQualifiedName,
            string FactoryMethodName,
            string SpecificationQualifiedType,
            string? ConstructedSpecificationReference,
            string SpecContainerCollectionQualifiedType,
            string SpecContainerCollectionReferenceName,
            string? InstanceHolderReference,
            IEnumerable<SpecContainerFactoryMethodInvocationTemplate> Arguments,
            Location Location
    ) : ISpecContainerMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal {ReturnTypeQualifiedName} {FactoryMethodName}(")
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
            writer.Append($"{referenceName}.{FactoryMethodName}");
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

        public class Builder {
            private readonly CreateSpecContainerFactoryMethodInvocationTemplate
                    createSpecContainerFactoryMethodInvocationTemplate;

            public Builder(CreateSpecContainerFactoryMethodInvocationTemplate createSpecContainerFactoryMethodInvocationTemplate) {
                this.createSpecContainerFactoryMethodInvocationTemplate = createSpecContainerFactoryMethodInvocationTemplate;
            }

            public SpecContainerFactoryMethodTemplate Build(
                    SpecContainerFactoryMethodDefinition specContainerFactoryMethodDefinition
            ) {
                var specContainerCollectionReferenceName = "specContainers";
                var instanceHolderReference = specContainerFactoryMethodDefinition.InstanceHolder?.ReferenceName ?? "";

                var arguments = specContainerFactoryMethodDefinition.Arguments.Select(
                        argument => createSpecContainerFactoryMethodInvocationTemplate(
                                argument,
                                specContainerCollectionReferenceName))
                        .ToImmutableList();

                return new SpecContainerFactoryMethodTemplate(
                        specContainerFactoryMethodDefinition.ProvidedType.QualifiedName,
                        specContainerFactoryMethodDefinition.MethodName,
                        specContainerFactoryMethodDefinition.SpecReference.SpecType.QualifiedName,
                        specContainerFactoryMethodDefinition.SpecReference.SpecReferenceName,
                        specContainerFactoryMethodDefinition.SpecContainerCollectionType.QualifiedName,
                        specContainerCollectionReferenceName,
                        instanceHolderReference,
                        arguments,
                        specContainerFactoryMethodDefinition.Location);
            }
        }
    }
}
