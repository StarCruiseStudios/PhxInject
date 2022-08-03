// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyProviderMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.External.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Specifications.Templates;

    // delegate ExternalDependencyProviderMethodTemplate CreateExternalDependencyProviderMethodTemplate(
    //         ExternalDependencyProviderMethodDefinition providerMethodDefinition,
    //         string specContainerCollectionReferenceName);

    internal record ExternalDependencyProviderMethodTemplate(
            string ReturnTypeQualifiedName,
            string ProviderMethodName,
            SpecContainerFactoryInvocationTemplate FactoryInvocationTemplate,
            Location Location) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public {ReturnTypeQualifiedName} {ProviderMethodName}() {{")
                    .IncreaseIndent(1)
                    .Append("return ");
            FactoryInvocationTemplate.Render(writer);
            writer.AppendLine(";")
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }

        // public class Builder {
        //     private readonly CreateExternalDependencySpecFactoryInvocationDefinition createFactoryMethodInvocation;
        //
        //     public Builder(CreateExternalDependencySpecFactoryInvocationDefinition createFactoryMethodInvocation) {
        //         this.createFactoryMethodInvocation = createFactoryMethodInvocation;
        //     }
        //
        //     public ExternalDependencyProviderMethodTemplate Build(
        //             ExternalDependencyProviderMethodDefinition providerMethodDefinition,
        //             string specContainerCollectionReferenceName
        //     ) {
        //         SpecContainerFactoryInvocationTemplate specContainerFactoryInvocation = null!;
        //                 // createFactoryMethodInvocation(
        //                 // providerMethodDefinition.SpecContainerFactoryInvocation,
        //                 // specContainerCollectionReferenceName);
        //         return new ExternalDependencyProviderMethodTemplate(
        //                 providerMethodDefinition.ProvidedType.QualifiedName,
        //                 providerMethodDefinition.ProviderMethodName,
        //                 specContainerFactoryInvocation,
        //                 providerMethodDefinition.Location);
        //     }
        // }
    }
}
