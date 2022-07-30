// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderMethodTemplate.cs" company="Star Cruise Studios LLC">
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

            writer.AppendLine($"{SpecificationQualifiedType}.{BuilderMethodName}(")
                    .IncreaseIndent(1)
                    .Append($"{BuiltInstanceReferenceName}");

            var numArguments = Arguments.Count();
            if (numArguments > 0) {
                foreach (var argument in Arguments) {
                    writer.AppendLine(",");
                    argument.Render(writer);
                }
            }

            writer.AppendLine(");")
                    .DecreaseIndent(2)
                    .AppendLine("}");
        }

        public class Builder {
            private readonly CreateSpecContainerFactoryMethodInvocationTemplate
                    createSpecContainerFactoryMethodInvocationTemplate;

            public Builder(CreateSpecContainerFactoryMethodInvocationTemplate createSpecContainerFactoryMethodInvocationTemplate) {
                this.createSpecContainerFactoryMethodInvocationTemplate = createSpecContainerFactoryMethodInvocationTemplate;
            }

            public SpecContainerBuilderMethodTemplate Build(
                    SpecContainerBuilderMethodDefinition specContainerBuilderMethodDefinition
            ) {
                var specContainerCollectionReferenceName = "specContainers";
                var builtInstanceReferenceName = "value";
                var arguments = specContainerBuilderMethodDefinition.Arguments.Select(
                                argument => createSpecContainerFactoryMethodInvocationTemplate(
                                        argument,
                                        specContainerCollectionReferenceName))
                        .ToImmutableList();

                return new SpecContainerBuilderMethodTemplate(
                        specContainerBuilderMethodDefinition.BuiltType.QualifiedName,
                        specContainerBuilderMethodDefinition.MethodName,
                        builtInstanceReferenceName,
                        specContainerBuilderMethodDefinition.SpecReference.SpecType.QualifiedName,
                        specContainerBuilderMethodDefinition.SpecContainerCollectionType.QualifiedName,
                        specContainerCollectionReferenceName,
                        arguments,
                        specContainerBuilderMethodDefinition.Location);
            }
        }
    }
}
