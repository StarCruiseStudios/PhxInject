// -----------------------------------------------------------------------------
//  <copyright file="DependencyImplementationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Project.Templates;

internal record DependencyImplementationTemplate(
    string DependencyImplementationClassName,
    string DependencyInterfaceQualifiedName,
    string InjectorSpecContainerCollectionQualifiedType,
    string SpecContainerCollectionReferenceName,
    IEnumerable<DependencyProviderMethodTemplate> DependencyProviderMethods,
    Location Location
) : IRenderTemplate {
    public void Render(IRenderWriter writer, RenderContext renderCtx) {
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
            method.Render(writer, renderCtx);
        }

        //  }
        writer.DecreaseIndent(1)
            .AppendLine("}");
    }

    internal interface IProjector {
        DependencyImplementationTemplate Project(
            DependencyImplementationDef dependencyImplementationDef,
            TemplateGenerationContext context
        );
    }

    public class Projector : IProjector {
        public DependencyImplementationTemplate Project(
            DependencyImplementationDef dependencyImplementationDef,
            TemplateGenerationContext context
        ) {
            var specContainerCollectionReferenceName = "specContainers";
            IReadOnlyList<DependencyProviderMethodTemplate> providerMethods =
                dependencyImplementationDef.ProviderMethodDefs.Select(provider => {
                        IReadOnlyList<SpecContainerFactorySingleInvocationTemplate> singleInvocationTemplates =
                            provider.SpecContainerFactoryInvocation.FactoryInvocationDefs.Select(def => {
                                    return new SpecContainerFactorySingleInvocationTemplate(
                                        specContainerCollectionReferenceName,
                                        def.SpecContainerType.GetPropertyName(),
                                        def.FactoryMethodName,
                                        def.Location
                                    );
                                })
                                .ToImmutableList();

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
                    })
                    .ToImmutableList();

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
