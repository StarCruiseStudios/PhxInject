// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    // internal delegate SpecContainerTemplate CreateSpecContainerTemplate(SpecContainerDefinition specContainerDefinition);

    internal record SpecContainerTemplate(
            string SpecContainerClassName,
            IEnumerable<SpecContainerInstanceHolderDeclarationTemplate> InstanceHolderDeclarations,
            SpecContainerConstructedSpecPropertyDeclarationTemplate? ConstructedSpecPropertyDeclaration,
            SpecContainerConstructorTemplate? Constructor,
            IEnumerable<ISpecContainerMemberTemplate> MemberTemplates,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal class {SpecContainerClassName} {{")
                    .IncreaseIndent(1);
            
            foreach (var instanceHolderDeclarationTemplate in InstanceHolderDeclarations) {
                instanceHolderDeclarationTemplate.Render(writer);
            }

            ConstructedSpecPropertyDeclaration?.Render(writer);
            Constructor?.Render(writer);

            foreach (var memberTemplate in MemberTemplates) {
                writer.AppendBlankLine();
                memberTemplate.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        // public class Builder {
        //     private readonly CreateInstanceHolderDeclarationTemplate createInstanceHolderDeclarationTemplate;
        //     private readonly CreateSpecContainerFactoryMethodTemplate createSpecContainerFactoryMethodTemplate;
        //     private readonly CreateSpecContainerBuilderMethodTemplate createSpecContainerBuilderMethodTemplate;
        //
        //     public Builder(
        //             CreateInstanceHolderDeclarationTemplate createInstanceHolderDeclarationTemplate,
        //             CreateSpecContainerFactoryMethodTemplate createSpecContainerFactoryMethodTemplate,
        //             CreateSpecContainerBuilderMethodTemplate createSpecContainerBuilderMethodTemplate
        //     ) {
        //         this.createSpecContainerBuilderMethodTemplate = createSpecContainerBuilderMethodTemplate;
        //         this.createSpecContainerFactoryMethodTemplate = createSpecContainerFactoryMethodTemplate;
        //         this.createInstanceHolderDeclarationTemplate = createInstanceHolderDeclarationTemplate;
        //     }
        //
        //     public SpecContainerTemplate Build(SpecContainerDefinition specContainerDefinition) {
        //         var constructedSpecClassQualifiedName = specContainerDefinition.SpecReference.InstantiationMode switch {
        //             SpecInstantiationMode.Instantiated => specContainerDefinition.SpecReference.SpecType.QualifiedName,
        //             SpecInstantiationMode.Static => "",
        //             _ => throw new InjectionException(
        //                     Diagnostics.UnexpectedError,
        //                     $"Unhandled SpecInstantiationMode {specContainerDefinition.SpecReference.InstantiationMode}",
        //                     specContainerDefinition.Location)
        //         };
        //
        //         var instanceHolderDeclarations = specContainerDefinition.InstanceHolderDeclarations.Select(
        //                         instanceHolderDeclaration =>
        //                                 createInstanceHolderDeclarationTemplate(instanceHolderDeclaration))
        //                 .ToImmutableList();
        //
        //         var memberTemplates = specContainerDefinition.FactoryMethodDefinitions
        //                 .Select(factoryMethod => createSpecContainerFactoryMethodTemplate(factoryMethod))
        //                 .Concat<ISpecContainerMemberTemplate>(
        //                         specContainerDefinition.BuilderMethodDefinitions
        //                                 .Select(builderMethod => createSpecContainerBuilderMethodTemplate(builderMethod)))
        //                 .ToImmutableList();
        //
        //         return new SpecContainerTemplate(
        //                 specContainerDefinition.ContainerType.TypeName,
        //                 constructedSpecClassQualifiedName,
        //                 specContainerDefinition.SpecReference.SpecReferenceName,
        //                 instanceHolderDeclarations,
        //                 memberTemplates,
        //                 specContainerDefinition.Location);
        //     }
        // }
    }
}
