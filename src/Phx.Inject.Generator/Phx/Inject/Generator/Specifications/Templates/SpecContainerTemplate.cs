// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Templates {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Templates;
    using Phx.Inject.Generator.Specifications.Definitions;

    internal delegate SpecContainerTemplate CreateSpecContainerTemplate(
            SpecContainerDefinition specContainerDefinition,
            TemplateGenerationContext context
    );

    internal record SpecContainerTemplate(
            string SpecContainerClassName,
            string? ConstructedSpecInterfaceQualifiedType,
            string? ConstructedSpecInstanceReferenceName,
            IEnumerable<SpecContainerInstanceHolder> InstanceHolders,
            IEnumerable<ISpecContainerMemberTemplate> MemberTemplates,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            //  internal class SpecContainerClassName {
            writer.AppendLine($"internal class {SpecContainerClassName} {{")
                    .IncreaseIndent(1);

            //      private ScopedInstanceType? scopedInstanceType;
            foreach (var instanceHolder in InstanceHolders) {
                writer.AppendLine($"private {instanceHolder.InstanceQualifiedType}? {instanceHolder.ReferenceName};");
            }

            //      private readonly ConstructedSpecificationInterfaceQualifiedType instance;
            //
            //      public SpecContainerClassName(ConstructedSpecificationInterfaceQualifiedType instance) {
            //          this.instance = instance;
            //      }
            if (ConstructedSpecInterfaceQualifiedType != null) {
                writer.AppendLine(
                                $"private readonly {ConstructedSpecInterfaceQualifiedType} {ConstructedSpecInstanceReferenceName};")
                        .AppendBlankLine()
                        .AppendLine(
                                $"public {SpecContainerClassName}({ConstructedSpecInterfaceQualifiedType} {ConstructedSpecInstanceReferenceName}) {{")
                        .IncreaseIndent(1)
                        .AppendLine(
                                $"this.{ConstructedSpecInstanceReferenceName} = {ConstructedSpecInstanceReferenceName};")
                        .DecreaseIndent(1)
                        .AppendLine("}");
            }

            //      public FactoryType GetFactoryType(
            //              SpecContainerCollectionType specContainers
            //      ) {
            //          return SpecificationType.GetFactoryType(
            //                  specContainers.SomeSpecContainer.GetDependency(specContainers));
            //      }
            //
            //      public ScopedInstanceType GetScopedInstanceType(
            //              SpecContainerCollectionType specContainers
            //      ) {
            //          return instance.GetScopedInstanceType(
            //                  specContainers.SomeSpecContainer.GetDependency(specContainers));
            //      }
            //
            //      public void BuildLazyType(
            //              LazyType value,
            //              SpecContainerCollectionType specContainers
            //      ) {
            //          SpecificationType.BuildLazyType(
            //                  value,
            //                  specContainers.SomeSpecContainer.GetDependency(specContainers));
            //      }
            //
            //      public ConstructorFactoryType GetConstructorFactoryType(
            //              SpecContainerCollectionType specContainers
            //      ) {
            //          return new ConstructorFactoryTYpe(
            //                  specContainers.SomeSpecContainer.GetDependency(specContainers));
            //      }
            foreach (var memberTemplate in MemberTemplates) {
                writer.AppendBlankLine();
                memberTemplate.Render(writer);
            }

            //  }
            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            private const string SpecReferenceName = "instance";
            private const string SpecContainerCollectionReferenceName = "specContainers";
            private const string BuiltInstanceReferenceName = "target";

            public SpecContainerTemplate Build(
                    SpecContainerDefinition specContainerDefinition,
                    TemplateGenerationContext context
            ) {
                string? constructedSpecInterfaceQualifiedType = null;
                string? constructedSpecificationReference = null;
                if (specContainerDefinition.SpecInstantiationMode == SpecInstantiationMode.Instantiated) {
                    constructedSpecInterfaceQualifiedType = specContainerDefinition.SpecificationType.QualifiedName;
                    constructedSpecificationReference = SpecReferenceName;
                }

                var instanceHolders = new List<SpecContainerInstanceHolder>();
                var memberTemplates = new List<ISpecContainerMemberTemplate>();

                // Create factory methods and instance holder declarations.
                foreach (var factoryMethod in specContainerDefinition.FactoryMethodDefinitions) {
                    string? instanceHolderReferenceName = null;
                    if (factoryMethod.FabricationMode == SpecFactoryMethodFabricationMode.Scoped) {
                        instanceHolderReferenceName = factoryMethod.ReturnType.GetVariableName();
                        instanceHolders.Add(
                                new SpecContainerInstanceHolder(
                                        factoryMethod.ReturnType.TypeModel.QualifiedName,
                                        instanceHolderReferenceName));
                    }

                    var arguments = factoryMethod.Arguments
                            .Select(
                                    argument => new SpecContainerFactoryInvocationTemplate(
                                            SpecContainerCollectionReferenceName,
                                            argument.SpecContainerType.GetPropertyName(),
                                            argument.FactoryMethodName,
                                            argument.RuntimeFactoryProvidedType?.QualifiedName,
                                            argument.Location))
                            .ToImmutableList();

                    memberTemplates.Add(
                            new SpecContainerFactoryTemplate(
                                    factoryMethod.ReturnType.TypeModel.QualifiedName,
                                    factoryMethod.SpecContainerFactoryMethodName,
                                    factoryMethod.SpecFactoryMemberName,
                                    factoryMethod.SpecFactoryMemberType,
                                    context.Injector.SpecContainerCollectionType.QualifiedName,
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
                                            argument.SpecContainerType.GetPropertyName(),
                                            argument.FactoryMethodName,
                                            argument.RuntimeFactoryProvidedType?.QualifiedName,
                                            argument.Location))
                            .ToImmutableList();

                    memberTemplates.Add(
                            new SpecContainerBuilderTemplate(
                                    builderMethod.BuiltType.QualifiedName,
                                    builderMethod.SpecContainerBuilderMethodName,
                                    builderMethod.SpecBuilderMemberName,
                                    builderMethod.SpecBuilderMemberType,
                                    BuiltInstanceReferenceName,
                                    context.Injector.SpecContainerCollectionType.QualifiedName,
                                    SpecContainerCollectionReferenceName,
                                    constructedSpecificationReference,
                                    specContainerDefinition.SpecificationType.QualifiedName,
                                    arguments,
                                    builderMethod.Location));
                }

                return new SpecContainerTemplate(
                        specContainerDefinition.SpecContainerType.TypeName,
                        constructedSpecInterfaceQualifiedType,
                        constructedSpecificationReference,
                        instanceHolders,
                        memberTemplates,
                        specContainerDefinition.Location);
            }
        }
    }
}
