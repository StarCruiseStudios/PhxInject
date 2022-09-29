// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyImplementationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.External.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Templates;
    using Phx.Inject.Generator.External.Definitions;
    using Phx.Inject.Generator.Specifications.Templates;

    internal delegate ExternalDependencyImplementationTemplate CreateExternalDependencyImplementationTemplate(
            ExternalDependencyImplementationDefinition externalDependencyImplementationDefinition,
            TemplateGenerationContext context
    );

    internal record ExternalDependencyImplementationTemplate(
            string ExternalDependencyImplementationClassName,
            string ExternalDependencyInterfaceQualifiedName,
            string InjectorSpecContainerCollectionQualifiedType,
            string SpecContainerCollectionReferenceName,
            IEnumerable<ExternalDependencyProviderMethodTemplate> ExternalDependencyProviderMethods,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            //  internal class ExternalDependencyImplementationClassName : ExternalDependencyInterfaceQualifiedType {
            writer.AppendLine(
                            $"internal class {ExternalDependencyImplementationClassName} : {ExternalDependencyInterfaceQualifiedName} {{")
                    .IncreaseIndent(1);

            //      private readonly InjectorSpecContainerCollectionQualifiedType specContainers;
            writer.AppendLine(
                    $"private readonly {InjectorSpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName};");

            //      public ExternalDependencyImplementationClassName(InjectorSpecContainerCollectionQualifiedType specContainers) {
            //          this.specContainers = specContainers;
            //      }
            writer.AppendBlankLine()
                    .AppendLine(
                            $"public {ExternalDependencyImplementationClassName}({InjectorSpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}) {{")
                    .IncreaseIndent(1)
                    .AppendLine(
                            $"this.{SpecContainerCollectionReferenceName} = {SpecContainerCollectionReferenceName};")
                    .DecreaseIndent(1)
                    .AppendLine("}");

            //      public DependencyType GetDependency() {
            //          return specContainers.SpecContainerReference.GetDependency(specContainers);
            //      }
            foreach (var method in ExternalDependencyProviderMethods) {
                writer.AppendBlankLine();
                method.Render(writer);
            }

            //  }
            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            public ExternalDependencyImplementationTemplate Build(
                    ExternalDependencyImplementationDefinition externalDependencyImplementationDefinition,
                    TemplateGenerationContext context
            ) {
                var specContainerCollectionReferenceName = "specContainers";
                var providerMethods = externalDependencyImplementationDefinition.ProviderMethodDefinitions.Select(
                        provider => {
                            var factoryInvocation = new SpecContainerFactoryInvocationTemplate(
                                    specContainerCollectionReferenceName,
                                    provider.SpecContainerFactoryInvocation.SpecContainerType.GetPropertyName(),
                                    provider.SpecContainerFactoryInvocation.FactoryMethodName,
                                    provider.Location);

                            return new ExternalDependencyProviderMethodTemplate(
                                    provider.ProvidedType.QualifiedName,
                                    provider.ProviderMethodName,
                                    factoryInvocation,
                                    provider.Location);
                        });

                return new ExternalDependencyImplementationTemplate(
                        externalDependencyImplementationDefinition.ExternalDependencyImplementationType.TypeName,
                        externalDependencyImplementationDefinition.ExternalDependencyInterfaceType.QualifiedName,
                        context.Injector.SpecContainerCollectionType.QualifiedName,
                        specContainerCollectionReferenceName,
                        providerMethods,
                        externalDependencyImplementationDefinition.Location);
            }
        }
    }
}
