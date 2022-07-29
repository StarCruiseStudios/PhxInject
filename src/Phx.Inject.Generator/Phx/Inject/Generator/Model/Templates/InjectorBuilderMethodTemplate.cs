// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate InjectorBuilderMethodTemplate CreateInjectorBuilderMethodTemplate(
            InjectorBuilderMethodDefinition injectorBuilderMethodDefinition,
            string specContainerCollectionReferenceName
    );

    internal record InjectorBuilderMethodTemplate(
            string BuiltTypeQualifiedName,
            string MethodName,
            SpecContainerBuilderMethodInvocationTemplate BuilderInvocationTemplate,
            string BuilderTargetName,
            Location Location
    ) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public void {MethodName}({BuiltTypeQualifiedName} {BuilderTargetName}) {{")
                    .IncreaseIndent(1);
            BuilderInvocationTemplate.Render(writer);
            writer.AppendLine(";")
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            private readonly CreateSpecContainerBuilderMethodInvocationTemplate
                    createSpecContainerBuilderMethodInvocationTemplate;

            public Builder(CreateSpecContainerBuilderMethodInvocationTemplate createSpecContainerBuilderMethodInvocationTemplate) {
                this.createSpecContainerBuilderMethodInvocationTemplate = createSpecContainerBuilderMethodInvocationTemplate;
            }

            public InjectorBuilderMethodTemplate Build(
                    InjectorBuilderMethodDefinition injectorBuilderMethodDefinition,
                    string specContainerCollectionReferenceName
            ) {
                var builderTargetName = "value";
                var builderMethodInvocation
                        = createSpecContainerBuilderMethodInvocationTemplate(
                                injectorBuilderMethodDefinition.SpecContainerBuilderInvocation,
                                specContainerCollectionReferenceName,
                                builderTargetName);

                return new InjectorBuilderMethodTemplate(
                        injectorBuilderMethodDefinition.BuiltType.QualifiedName,
                        injectorBuilderMethodDefinition.InjectorMethodName,
                        builderMethodInvocation,
                        builderTargetName,
                        injectorBuilderMethodDefinition.Location);
            }
        }
    }
}
