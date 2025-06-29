// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Definitions;
    using Phx.Inject.Generator.Model;

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
            var hasContainerScopedReferences = InstanceHolders.Any(it => it.isContainerScoped);

            //  internal class SpecContainerClassName {
            writer.AppendLine($"internal class {SpecContainerClassName} {{")
                .IncreaseIndent(1);
            if (hasContainerScopedReferences) {
                //      private SpecContainerClassName? parent;
                writer.AppendLine($"private {SpecContainerClassName}? parent;");
            }

            //      private ScopedInstanceType? scopedInstanceType;
            foreach (var instanceHolder in InstanceHolders) {
                // instanceHolder.isContainerScoped
                if (!hasContainerScopedReferences || instanceHolder.isContainerScoped) {
                    writer.AppendLine(
                        $"private {instanceHolder.InstanceQualifiedType}? {instanceHolder.ReferenceName};");
                } else {
                    writer.AppendLine(
                            $"private {instanceHolder.InstanceQualifiedType}? _{instanceHolder.ReferenceName};")
                        .Append($"private {instanceHolder.InstanceQualifiedType}? {instanceHolder.ReferenceName} {{")
                        .IncreaseIndent(1)
                        .AppendLine()
                        .AppendLine($"get {{ return _{instanceHolder.ReferenceName}; }}")
                        .Append("set {")
                        .IncreaseIndent(1)
                        .AppendLine()
                        .AppendLine($"this._{instanceHolder.ReferenceName} = value;")
                        .Append("if (parent != null) {")
                        .IncreaseIndent(1)
                        .AppendLine()
                        .Append($"parent.{instanceHolder.ReferenceName} = value;")
                        .DecreaseIndent(1)
                        .AppendLine()
                        .Append("}")
                        .DecreaseIndent(1)
                        .AppendLine()
                        .Append("}")
                        .DecreaseIndent(1)
                        .AppendLine()
                        .Append("}");
                }
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

            //      internal SpecContainerClassName CreateNewFrame() {
            //          var newFrame = new SpecContainerClassName();
            //          newFrame.scopedInstanceType = this.scopedInstanceType;
            //          newFrame.parent = this;
            //          return newFrame;
            //      }
            writer.AppendBlankLine()
                .Append($"internal {SpecContainerClassName} CreateNewFrame() {{")
                .IncreaseIndent(1)
                .AppendLine();

            if (hasContainerScopedReferences) {
                if (ConstructedSpecInterfaceQualifiedType != null) {
                    writer.AppendLine(
                        $"var newFrame = new {SpecContainerClassName}(this.{ConstructedSpecInstanceReferenceName});");
                } else {
                    writer.AppendLine($"var newFrame = new {SpecContainerClassName}();");
                }

                foreach (var instanceHolder in InstanceHolders) {
                    if (!instanceHolder.isContainerScoped) {
                        writer.AppendLine(
                            $"newFrame.{instanceHolder.ReferenceName} = this.{instanceHolder.ReferenceName};");
                    }
                }

                writer.AppendLine("newFrame.parent = this;")
                    .Append("return newFrame;");
            } else {
                writer.Append("return this;");
            }

            writer.DecreaseIndent(1)
                .AppendLine()
                .AppendLine("}");

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
                    if (factoryMethod.FabricationMode == SpecFactoryMethodFabricationMode.Scoped
                        || factoryMethod.FabricationMode == SpecFactoryMethodFabricationMode.ContainerScoped) {
                        instanceHolderReferenceName = factoryMethod.ReturnType.GetVariableName();
                        instanceHolders.Add(
                            new SpecContainerInstanceHolder(
                                factoryMethod.ReturnType.TypeModel.QualifiedName,
                                instanceHolderReferenceName,
                                isContainerScoped: factoryMethod.FabricationMode
                                == SpecFactoryMethodFabricationMode.ContainerScoped));
                    }

                    var arguments = factoryMethod.Arguments
                        .Select(argument => createSpecContainerFactoryInvocationTemplate(argument))
                        .ToImmutableList();
                    var requiredProperties = factoryMethod.RequiredProperties
                        .Select(property => new RequiredPropertyTemplate(
                            property.PropertyName,
                            createSpecContainerFactoryInvocationTemplate(property.Value)))
                        .ToImmutableList();

                    var startNewContainer = factoryMethod.FabricationMode == SpecFactoryMethodFabricationMode.Container;

                    memberTemplates.Add(
                        new SpecContainerFactoryTemplate(
                            factoryMethod.ReturnType.TypeModel.QualifiedName,
                            factoryMethod.SpecContainerFactoryMethodName,
                            factoryMethod.SpecFactoryMemberName,
                            factoryMethod.SpecFactoryMemberType,
                            context.Injector.SpecContainerCollectionType.QualifiedName,
                            SpecContainerCollectionReferenceName,
                            instanceHolderReferenceName,
                            startNewContainer,
                            constructedSpecificationReference,
                            specContainerDefinition.SpecificationType.QualifiedName,
                            arguments,
                            requiredProperties,
                            specContainerDefinition.Location
                        )
                    );
                }

                // Create builder methods.
                foreach (var builderMethod in specContainerDefinition.BuilderMethodDefinitions) {
                    var arguments = builderMethod.Arguments
                        .Select(argument => {
                            var singleInvocationTemplates =
                                argument.FactoryInvocationDefinitions.Select(
                                    def => {
                                        return new SpecContainerFactorySingleInvocationTemplate(
                                            SpecContainerCollectionReferenceName,
                                            def.SpecContainerType.GetPropertyName(),
                                            def.FactoryMethodName,
                                            def.Location
                                        );
                                    }).ToList();

                            string? multiBindQualifiedTypeArgs = null;
                            if (argument.FactoryInvocationDefinitions.Count > 1) {
                                multiBindQualifiedTypeArgs =
                                    TypeHelpers.GetQualifiedTypeArgs(
                                        argument.FactoryReturnType);
                            }

                            return new SpecContainerFactoryInvocationTemplate(
                                singleInvocationTemplates,
                                multiBindQualifiedTypeArgs,
                                argument.RuntimeFactoryProvidedType?.QualifiedName,
                                argument.Location);
                        })
                        .ToImmutableList();


                    var specificationQualifiedType = constructedSpecificationReference ??
                        (builderMethod.SpecBuilderMemberType == SpecBuilderMemberType.Direct
                            ? builderMethod.BuiltType.QualifiedName
                            : specContainerDefinition.SpecificationType.QualifiedName); 
                    
                    
                    memberTemplates.Add(
                        new SpecContainerBuilderTemplate(
                            builderMethod.BuiltType.QualifiedName,
                            builderMethod.SpecContainerBuilderMethodName,
                            builderMethod.SpecBuilderMemberName,
                            BuiltInstanceReferenceName,
                            context.Injector.SpecContainerCollectionType.QualifiedName,
                            SpecContainerCollectionReferenceName,
                            specificationQualifiedType,
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
            
            private SpecContainerFactoryInvocationTemplate createSpecContainerFactoryInvocationTemplate(
                SpecContainerFactoryInvocationDefinition argument
            ) {
                var singleInvocationTemplates =
                    argument.FactoryInvocationDefinitions.Select(
                        def => {
                            return new SpecContainerFactorySingleInvocationTemplate(
                                SpecContainerCollectionReferenceName,
                                def.SpecContainerType.GetPropertyName(),
                                def.FactoryMethodName,
                                def.Location
                            );
                        }).ToList();

                string? multiBindQualifiedTypeArgs = null;
                if (argument.FactoryInvocationDefinitions.Count > 1) {
                    multiBindQualifiedTypeArgs =
                        TypeHelpers.GetQualifiedTypeArgs(
                            argument.FactoryReturnType);
                }

                return new SpecContainerFactoryInvocationTemplate(
                    singleInvocationTemplates,
                    multiBindQualifiedTypeArgs,
                    argument.RuntimeFactoryProvidedType?.QualifiedName,
                    argument.Location);
            }
        }
    }
}
