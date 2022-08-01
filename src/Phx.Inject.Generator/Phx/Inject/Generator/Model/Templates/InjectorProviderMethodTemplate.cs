﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    delegate InjectorProviderMethodTemplate CreateInjectorProviderMethodTemplate(
            InjectorProviderMethodDefinition injectorProviderMethodDefinition,
            string specContainerCollectionReferenceName
    );

    internal record InjectorProviderMethodTemplate(
            string ReturnTypeQualifiedName,
            string MethodName,
            SpecContainerFactoryMethodInvocationTemplate FactoryInvocationTemplate,
            Location Location
    ) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public {ReturnTypeQualifiedName} {MethodName}() {{")
                    .IncreaseIndent(1)
                    .Append("return ");
            FactoryInvocationTemplate.Render((writer));
            writer.AppendLine(";")
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            private readonly CreateSpecContainerFactoryMethodInvocationTemplate
                    createSpecContainerFactoryMethodInvocationTemplate;

            public Builder(CreateSpecContainerFactoryMethodInvocationTemplate createSpecContainerFactoryMethodInvocationTemplate) {
                this.createSpecContainerFactoryMethodInvocationTemplate = createSpecContainerFactoryMethodInvocationTemplate;
            }

            public InjectorProviderMethodTemplate Build(
                    InjectorProviderMethodDefinition injectorProviderMethodDefinition,
                    string specContainerCollectionReferenceName
            ) {
                var factoryMethodInvocation
                        = createSpecContainerFactoryMethodInvocationTemplate(
                                injectorProviderMethodDefinition.SpecContainerFactoryInvocation,
                                specContainerCollectionReferenceName);

                return new InjectorProviderMethodTemplate(
                        injectorProviderMethodDefinition.ProvidedType.QualifiedName,
                        injectorProviderMethodDefinition.InjectorMethodName,
                        factoryMethodInvocation,
                        injectorProviderMethodDefinition.Location);
            }
        }
    }
}