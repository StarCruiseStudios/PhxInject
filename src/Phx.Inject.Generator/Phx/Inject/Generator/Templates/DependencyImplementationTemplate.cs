// -----------------------------------------------------------------------------
//  <copyright file="DependencyImplementationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Definitions;

    internal record DependencyImplementationTemplate(
        string DependencyImplementationClassName,
        string DependencyInterfaceQualifiedName,
        string InjectorSpecContainerCollectionQualifiedType,
        string SpecContainerCollectionReferenceName,
        IEnumerable<DependencyProviderMethodTemplate> DependencyProviderMethods,
        Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            //  internal class DependencyImplementationClassName : DependencyInterfaceQualifiedType {
            writer.AppendLine(
                    $"internal class {DependencyImplementationClassName} : {DependencyInterfaceQualifiedName} {{")
                .IncreaseIndent(1);

            //      private readonly InjectorSpecContainerCollectionQualifiedType specContainers;
            writer.AppendLine(
                $"private readonly {InjectorSpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName};");

            //      public DependencyImplementationClassName(InjectorSpecContainerCollectionQualifiedType specContainers) {
            //          this.specContainers = specContainers;
            //      }
            writer.AppendBlankLine()
                .AppendLine(
                    $"public {DependencyImplementationClassName}({InjectorSpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}) {{")
                .IncreaseIndent(1)
                .AppendLine(
                    $"this.{SpecContainerCollectionReferenceName} = {SpecContainerCollectionReferenceName};")
                .DecreaseIndent(1)
                .AppendLine("}");

            //      public DependencyType GetDependency() {
            //          return specContainers.SpecContainerReference.GetDependency(specContainers);
            //      }
            foreach (var method in DependencyProviderMethods) {
                writer.AppendBlankLine();
                method.Render(writer);
            }

            //  }
            writer.DecreaseIndent(1)
                .AppendLine("}");
        }

        
        internal interface IBuilder {
            DependencyImplementationTemplate Build(
                DependencyImplementationDef dependencyImplementationDef,
                TemplateGenerationContext context
            );
        }
        
        public class Builder : IBuilder {
            public DependencyImplementationTemplate Build(
                DependencyImplementationDef dependencyImplementationDef,
                TemplateGenerationContext context
            ) {
                var specContainerCollectionReferenceName = "specContainers";
                var providerMethods = dependencyImplementationDef.ProviderMethodDefs.Select(
                    provider => {
                        var singleInvocationTemplates =
                            provider.SpecContainerFactoryInvocation.FactoryInvocationDefs.Select(
                                def => {
                                    return new SpecContainerFactorySingleInvocationTemplate(
                                        specContainerCollectionReferenceName,
                                        def.SpecContainerType.GetPropertyName(),
                                        def.FactoryMethodName,
                                        def.Location
                                    );
                                }).ToList();

                        string? multiBindQualifiedTypeArgs = null;
                        if (provider.SpecContainerFactoryInvocation.FactoryInvocationDefs.Count > 1) {
                            multiBindQualifiedTypeArgs =
                                TypeHelpers.GetQualifiedTypeArgs(
                                    provider.SpecContainerFactoryInvocation.FactoryReturnType);
                        }

                        var factoryInvocation = new SpecContainerFactoryInvocationTemplate(
                            singleInvocationTemplates,
                            multiBindQualifiedTypeArgs,
                            provider.SpecContainerFactoryInvocation.RuntimeFactoryProvidedType?.QualifiedName,
                            provider.Location);

                        return new DependencyProviderMethodTemplate(
                            provider.ProvidedType.QualifiedName,
                            provider.ProviderMethodName,
                            factoryInvocation,
                            provider.Location);
                    });

                return new DependencyImplementationTemplate(
                    dependencyImplementationDef.DependencyImplementationType.TypeName,
                    dependencyImplementationDef.DependencyInterfaceType.QualifiedName,
                    context.Injector.SpecContainerCollectionType.QualifiedName,
                    specContainerCollectionReferenceName,
                    providerMethods,
                    dependencyImplementationDef.Location);
            }
        }
    }
}
