﻿// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Project.Templates;

internal record SpecContainerTemplate(
    string SpecContainerClassName,
    string? ConstructedSpecInterfaceQualifiedType,
    string? ConstructedSpecInstanceReferenceName,
    IEnumerable<SpecContainerTemplate.SpecContainerInstanceHolder> InstanceHolders,
    IEnumerable<ISpecContainerMemberTemplate> MemberTemplates,
    Location Location
) : IRenderTemplate {
    public void Render(IRenderWriter writer, RenderContext renderCtx) {
        var hasContainerScopedReferences = InstanceHolders.Any(it => it.isContainerScoped);

        //  internal class SpecContainerClassName {
        writer.AppendLine($"internal class {SpecContainerClassName} {{")
            .IncreaseIndent(1);
        if (hasContainerScopedReferences) {
            //      private SpecContainerClassName? parent;
            writer.AppendLine($"private {SpecContainerClassName}? parent;");
        }

        //      private ScopedInstanceType? scopedInstanceType;
        foreach (var instanceHolder in InstanceHolders.OrderBy(it => it.ReferenceName)) {
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

            foreach (var instanceHolder in InstanceHolders.OrderBy(it => it.ReferenceName)) {
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
            memberTemplate.Render(writer, renderCtx);
        }

        //  }
        writer.DecreaseIndent(1)
            .AppendLine("}");
    }

    public record SpecContainerInstanceHolder(
        string InstanceQualifiedType,
        string ReferenceName,
        bool isContainerScoped
    );

    public interface IProjector {
        SpecContainerTemplate Project(
            SpecContainerDef specContainerDef,
            TemplateGenerationContext context
        );
    }

    public class Projector : IProjector {
        private const string SpecReferenceName = "instance";
        private const string SpecContainerCollectionReferenceName = "specContainers";
        private const string BuiltInstanceReferenceName = "target";

        public SpecContainerTemplate Project(
            SpecContainerDef specContainerDef,
            TemplateGenerationContext context
        ) {
            string? constructedSpecInterfaceQualifiedType = null;
            string? constructedSpecificationReference = null;
            if (specContainerDef.SpecInstantiationMode
                is SpecInstantiationMode.Instantiated
                or SpecInstantiationMode.Dependency
            ) {
                constructedSpecInterfaceQualifiedType = specContainerDef.SpecificationType.NamespacedName;
                constructedSpecificationReference = SpecReferenceName;
            }

            var instanceHolders = new List<SpecContainerInstanceHolder>();
            var memberTemplates = new List<ISpecContainerMemberTemplate>();

            // Create factory methods and instance holder declarations.
            foreach (var factoryMethod in specContainerDef.FactoryMethodDefs.OrderBy(it => it.SpecContainerFactoryMethodName)) {
                string? instanceHolderReferenceName = null;
                if (factoryMethod.FabricationMode == FactoryFabricationMode.Scoped
                    || factoryMethod.FabricationMode == FactoryFabricationMode.ContainerScoped) {
                    instanceHolderReferenceName = factoryMethod.ReturnType.GetVariableName();
                    instanceHolders.Add(
                        new SpecContainerInstanceHolder(
                            factoryMethod.ReturnType.TypeModel.NamespacedName,
                            instanceHolderReferenceName,
                            factoryMethod.FabricationMode
                            == FactoryFabricationMode.ContainerScoped));
                }

                IReadOnlyList<SpecContainerFactoryInvocationTemplate> arguments = factoryMethod.Arguments
                    .Select(argument => createSpecContainerFactoryInvocationTemplate(argument))
                    .ToImmutableList();
                IReadOnlyList<RequiredPropertyTemplate> requiredProperties = factoryMethod.RequiredProperties
                    .Select(property => new RequiredPropertyTemplate(
                        property.PropertyName,
                        createSpecContainerFactoryInvocationTemplate(property.Value)))
                    .ToImmutableList();

                var startNewContainer = factoryMethod.FabricationMode == FactoryFabricationMode.Container;

                memberTemplates.Add(
                    new SpecContainerFactoryTemplate(
                        factoryMethod.ReturnType.TypeModel.NamespacedName,
                        factoryMethod.SpecContainerFactoryMethodName,
                        factoryMethod.SpecFactoryMemberName,
                        factoryMethod.SpecFactoryMemberType,
                        context.Injector.SpecContainerCollectionType.NamespacedName,
                        SpecContainerCollectionReferenceName,
                        instanceHolderReferenceName,
                        startNewContainer,
                        constructedSpecificationReference,
                        specContainerDef.SpecificationType.NamespacedName,
                        arguments,
                        requiredProperties,
                        specContainerDef.Location
                    )
                );
            }

            // Create builder methods.
            foreach (var builderMethod in specContainerDef.BuilderMethodDefs.OrderBy(it => it.SpecContainerBuilderMethodName)) {
                IReadOnlyList<SpecContainerFactoryInvocationTemplate> arguments = builderMethod.Arguments
                    .Select(argument => {
                        IReadOnlyList<SpecContainerFactorySingleInvocationTemplate> singleInvocationTemplates =
                            argument.FactoryInvocationDefs.Select(def => {
                                    return new SpecContainerFactorySingleInvocationTemplate(
                                        SpecContainerCollectionReferenceName,
                                        def.SpecContainerType.GetPropertyName(),
                                        def.FactoryMethodName,
                                        def.Location
                                    );
                                })
                                .ToImmutableList();

                        string? multiBindQualifiedTypeArgs = null;
                        var isReadOnlySet = false;
                        if (argument.FactoryInvocationDefs.Count > 1) {
                            multiBindQualifiedTypeArgs =
                                TypeHelpers.GetQualifiedTypeArgs(
                                    argument.FactoryReturnType);
                            isReadOnlySet = argument.FactoryReturnType.TypeModel.NamespacedBaseTypeName
                                == TypeNames.IReadOnlySetClassName;
                        }

                        return new SpecContainerFactoryInvocationTemplate(
                            singleInvocationTemplates,
                            multiBindQualifiedTypeArgs,
                            isReadOnlySet,
                            argument.RuntimeFactoryProvidedType?.NamespacedName,
                            argument.Location);
                    })
                    .ToImmutableList();

                var specificationQualifiedType = constructedSpecificationReference
                    ?? (builderMethod.SpecBuilderMemberType == SpecBuilderMemberType.Direct
                        ? builderMethod.BuiltType.NamespacedName
                        : specContainerDef.SpecificationType.NamespacedName);

                memberTemplates.Add(
                    new SpecContainerBuilderTemplate(
                        builderMethod.BuiltType.NamespacedName,
                        builderMethod.SpecContainerBuilderMethodName,
                        builderMethod.SpecBuilderMemberName,
                        BuiltInstanceReferenceName,
                        context.Injector.SpecContainerCollectionType.NamespacedName,
                        SpecContainerCollectionReferenceName,
                        specificationQualifiedType,
                        arguments,
                        builderMethod.Location));
            }

            return new SpecContainerTemplate(
                specContainerDef.SpecContainerType.TypeName,
                constructedSpecInterfaceQualifiedType,
                constructedSpecificationReference,
                instanceHolders,
                memberTemplates,
                specContainerDef.Location);
        }

        private SpecContainerFactoryInvocationTemplate createSpecContainerFactoryInvocationTemplate(
            SpecContainerFactoryInvocationDef argument
        ) {
            IReadOnlyList<SpecContainerFactorySingleInvocationTemplate> singleInvocationTemplates =
                argument.FactoryInvocationDefs.Select(def => {
                        return new SpecContainerFactorySingleInvocationTemplate(
                            SpecContainerCollectionReferenceName,
                            def.SpecContainerType.GetPropertyName(),
                            def.FactoryMethodName,
                            def.Location
                        );
                    })
                    .ToImmutableList();

            string? multiBindQualifiedTypeArgs = null;
            var isReadOnlySet = false;
            if (argument.FactoryInvocationDefs.Count > 1) {
                multiBindQualifiedTypeArgs =
                    TypeHelpers.GetQualifiedTypeArgs(
                        argument.FactoryReturnType);
                isReadOnlySet = argument.FactoryReturnType.TypeModel.NamespacedBaseTypeName
                    == TypeNames.IReadOnlySetClassName;
            }

            return new SpecContainerFactoryInvocationTemplate(
                singleInvocationTemplates,
                multiBindQualifiedTypeArgs,
                isReadOnlySet,
                argument.RuntimeFactoryProvidedType?.NamespacedName,
                argument.Location);
        }
    }
}
