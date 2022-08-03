// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyImplementationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.External.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Injectors.Templates;

    // internal delegate ExternalDependencyImplementationTemplate CreateExternalDependencyContainerTemplate();

    internal record ExternalDependencyImplementationTemplate(
            string ExternalDependencyImplementationClassName,
            string ExternalDependencyInterfaceQualifiedName,
            InjectorSpecContainerCollectionReferenceDeclarationTemplate SpecContainerCollectionReferenceDeclaration,
            ExternalDependencyImplementationConstructorTemplate Constructor,
            IEnumerable<ExternalDependencyProviderMethodTemplate> ExternalDependencyProviderMethods,
            Location Location) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal class {ExternalDependencyImplementationClassName} : {ExternalDependencyInterfaceQualifiedName} {{")
                    .IncreaseIndent(1);
            SpecContainerCollectionReferenceDeclaration.Render(writer);
            writer.AppendBlankLine();
            Constructor.Render(writer);

            foreach (var method in ExternalDependencyProviderMethods) {
                writer.AppendBlankLine();
                method.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        // public class Builder {
        //     private readonly ExternalDependencyProviderMethodTemplate createProviderMethod;
        //
        //     public Builder(ExternalDependencyProviderMethodTemplate createProviderMethod) {
        //         this.createProviderMethod = createProviderMethod;
        //     }
        //
        //     public ExternalDependencyImplementationTemplate Build() {
        //         return new ExternalDependencyImplementationTemplate(
        //                 null!,
        //                 null!,
        //                 null!,
        //                 null!,
        //                 null!,
        //                 null!);
        //     }
        // }
    }
}
