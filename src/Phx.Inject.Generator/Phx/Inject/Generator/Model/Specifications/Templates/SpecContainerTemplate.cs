// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Templates {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Model.Specifications.Definitions;

    internal delegate SpecContainerTemplate CreateSpecContainerTemplate(
            SpecContainerDefinition specContainerDefinition,
            ITemplateGenerationContext context
    );

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

        public class Builder {
            private const string SpecReferenceName = "instance";
            private const string SpecContainerCollectionReferenceName = "specContainers";
            private const string BuiltInstanceReferenceName = "target";

            public SpecContainerTemplate Build(
                    SpecContainerDefinition specContainerDefinition,
                    ITemplateGenerationContext context
            ) {
                // Create constructed spec property declaration and constructor for instantiated specification types.
                SpecContainerConstructedSpecPropertyDeclarationTemplate? constructedSpecPropertyDeclaration = null;
                SpecContainerConstructorTemplate? specContainerConstructor = null;
                string? constructedSpecificationReference = null;
                if (specContainerDefinition.SpecInstantiationMode == SpecInstantiationMode.Instantiated) {
                    constructedSpecificationReference = SpecReferenceName;
                    constructedSpecPropertyDeclaration = new SpecContainerConstructedSpecPropertyDeclarationTemplate(
                            specContainerDefinition.SpecificationType.QualifiedName,
                            constructedSpecificationReference,
                            specContainerDefinition.Location);

                    var constructorParameters = ImmutableList.Create(
                            new SpecContainerConstructorParameterTemplate(
                                    specContainerDefinition.SpecificationType.QualifiedName,
                                    SpecReferenceName,
                                    specContainerDefinition.Location));
                    var constructorAssignments = ImmutableList.Create(
                            new SpecContainerConstructorAssignmentTemplate(
                                    constructedSpecificationReference,
                                    SpecReferenceName,
                                    specContainerDefinition.Location));

                    specContainerConstructor = new SpecContainerConstructorTemplate(
                            specContainerDefinition.SpecContainerType.TypeName,
                            constructorParameters,
                            constructorAssignments,
                            specContainerDefinition.Location);
                }

                var instanceHolderDeclarations = new List<SpecContainerInstanceHolderDeclarationTemplate>();
                var memberTemplates = new List<ISpecContainerMemberTemplate>();

                // Create factory methods and instance holder declarations.
                foreach (var factoryMethod in specContainerDefinition.FactoryMethodDefinitions) {
                    string? instanceHolderReferenceName = null;
                    if (factoryMethod.FabricationMode == SpecFactoryMethodFabricationMode.Scoped) {
                        instanceHolderReferenceName = "referenceName";
                        instanceHolderDeclarations.Add(
                                new SpecContainerInstanceHolderDeclarationTemplate(
                                        factoryMethod.ReturnType.QualifiedName,
                                        instanceHolderReferenceName,
                                        factoryMethod.Location));
                    }

                    var arguments = factoryMethod.Arguments
                            .Select(
                                    argument => new SpecContainerFactoryInvocationTemplate(
                                            SpecContainerCollectionReferenceName,
                                            SymbolProcessors.GetSpecContainerReferenceName(argument.SpecContainerType),
                                            argument.FactoryMethodName,
                                            argument.Location))
                            .ToImmutableList();

                    memberTemplates.Add(
                            new SpecContainerFactoryTemplate(
                                    factoryMethod.ReturnType.QualifiedName,
                                    factoryMethod.FactoryMethodName,
                                    factoryMethod.SpecContainerCollectionType.QualifiedName,
                                    SpecContainerCollectionReferenceName,
                                    instanceHolderReferenceName,
                                    constructedSpecificationReference,
                                    specContainerDefinition.SpecificationType.QualifiedName,
                                    arguments,
                                    specContainerDefinition.Location));
                }

                // Create builder methods.
                foreach (var builderMethod in specContainerDefinition.BuilderMethodDefinitions) {
                    var arguments = builderMethod.Arguments
                            .Select(
                                    argument => new SpecContainerFactoryInvocationTemplate(
                                            SpecContainerCollectionReferenceName,
                                            SymbolProcessors.GetSpecContainerReferenceName(argument.SpecContainerType),
                                            argument.FactoryMethodName,
                                            argument.Location))
                            .ToImmutableList();

                    memberTemplates.Add(
                            new SpecContainerBuilderTemplate(
                                    builderMethod.BuiltType.QualifiedName,
                                    builderMethod.MethodName,
                                    BuiltInstanceReferenceName,
                                    builderMethod.SpecContainerCollectionType.QualifiedName,
                                    SpecContainerCollectionReferenceName,
                                    constructedSpecificationReference,
                                    specContainerDefinition.SpecificationType.QualifiedName,
                                    arguments,
                                    builderMethod.Location));
                }

                return new SpecContainerTemplate(
                        specContainerDefinition.SpecContainerType.TypeName,
                        instanceHolderDeclarations,
                        constructedSpecPropertyDeclaration,
                        specContainerConstructor,
                        memberTemplates,
                        specContainerDefinition.Location);
            }
        }
    }
}
