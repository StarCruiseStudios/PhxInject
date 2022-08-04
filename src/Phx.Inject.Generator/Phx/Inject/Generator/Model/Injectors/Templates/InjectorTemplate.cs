// -----------------------------------------------------------------------------
//  <copyright file="InjectorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Model.Injectors.Definitions;
    using Phx.Inject.Generator.Model.Specifications.Templates;

    internal delegate InjectorTemplate CreateInjectorTemplate(
            InjectorDefinition injectorDefinition,
            TemplateGenerationContext context);

    internal record InjectorTemplate(
            string InjectorClassName,
            string InjectorInterfaceQualifiedName,
            InjectorSpecContainerCollectionDeclarationTemplate SpecContainerCollectionDeclaration,
            InjectorSpecContainerCollectionReferenceDeclarationTemplate SpecContainerCollectionReferenceDeclaration,
            InjectorConstructorTemplate InjectorConstructor,
            IEnumerable<IInjectorMemberTemplate> InjectorMemberTemplates,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal partial class {InjectorClassName} : {InjectorInterfaceQualifiedName} {{")
                    .IncreaseIndent(1);
            SpecContainerCollectionDeclaration.Render(writer);
            SpecContainerCollectionReferenceDeclaration.Render(writer);
            InjectorConstructor.Render(writer);

            foreach (var memberTemplate in InjectorMemberTemplates) {
                writer.AppendBlankLine();
                memberTemplate.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            private const string SpecContainerCollectionReferenceName = "specContainers";
            private const string SpecContainerCollectionClassName = "SpecContainerCollection";

            public InjectorTemplate Build(
                    InjectorDefinition injectorDefinition,
                    TemplateGenerationContext context
            ) {
                var specContainerCollectionPropertyDeclarations = injectorDefinition.Specifications.Select(
                        specType => {
                            var specContainerType = SymbolProcessors.CreateSpecContainerType(
                                    injectorDefinition.InjectorType,
                                    specType);
                            return new InjectorSpecContainerCollectionPropertyDeclarationTemplate(
                                    specContainerType.QualifiedName,
                                    SymbolProcessors.GetSpecContainerReferenceName(specContainerType),
                                    injectorDefinition.Location);
                        })
                        .ToImmutableList();
                var specContainerCollectionDeclaration = new InjectorSpecContainerCollectionDeclarationTemplate(
                        SpecContainerCollectionClassName,
                        specContainerCollectionPropertyDeclarations,
                        injectorDefinition.Location);
                var specContainerCollectionReferenceDeclaration
                        = new InjectorSpecContainerCollectionReferenceDeclarationTemplate(
                                SpecContainerCollectionClassName,
                                SpecContainerCollectionReferenceName,
                                injectorDefinition.Location);


                var s = injectorDefinition.Specifications.Select(
                        spec => {
                            context.SpecContainers.try
                        })


                var specContainerCollectionInitialization = new InjectorSpecContainerCollectionInitializationTemplate(
                        SpecContainerCollectionClassName,
                        SpecContainerCollectionClassName,
                        null!, // TODO:
                        injectorDefinition.Location);

                var injectorConstructor = new InjectorConstructorTemplate(
                        injectorDefinition.InjectorType.TypeName,
                        null!, // TODO:
                        specContainerCollectionInitialization,
                        injectorDefinition.Location);


                IEnumerable<IInjectorMemberTemplate> injectorMemberTemplates = injectorDefinition.Providers.Select(
                                provider => {
                                    var invocationDefinition = provider.SpecContainerFactoryInvocation;
                                    var factoryInvocation = new SpecContainerFactoryInvocationTemplate(
                                            SpecContainerCollectionReferenceName,
                                            SymbolProcessors.GetSpecContainerReferenceName(invocationDefinition.SpecContainerType),
                                            invocationDefinition.FactoryMethodName,
                                            invocationDefinition.Location);

                                    return new InjectorProviderTemplate(
                                            provider.ProvidedType.QualifiedName,
                                            provider.InjectorProviderMethodName,
                                            factoryInvocation,
                                            provider.Location);
                                })
                        .Concat<IInjectorMemberTemplate>(
                                injectorDefinition.Builders.Select(
                                        builder => {
                                            var builderTargetName = "target";
                                            var invocationDefinition = builder.SpecContainerBuilderInvocation;
                                            var builderInvocation = new SpecContainerBuilderInvocationTemplate(
                                                    SpecContainerCollectionReferenceName,
                                                    SymbolProcessors.GetSpecContainerReferenceName(invocationDefinition.SpecContainerType),
                                                    invocationDefinition.BuilderMethodName,
                                                    builderTargetName,
                                                    invocationDefinition.Location);

                                            return new InjectorBuilderTemplate(
                                                    builder.BuiltType.QualifiedName,
                                                    builder.InjectorBuilderMethodName,
                                                    builderTargetName,
                                                    builderInvocation,
                                                    builder.Location);
                                        }))
                        .Concat(
                                injectorDefinition.ChildFactories.Select(
                                        factory => {
                                            // Name of the generated child injector type
                                            var childTypeQualifiedName = "TODO";

                                            // Name of the generated class that implements the external dependency interface.
                                            var childExternalDependencyImplementationTypeQualifiedName = "TODO";

                                            return new InjectorChildFactoryTemplate(
                                                    factory.InjectorChildInterfaceType.QualifiedName,
                                                    factory.InjectorChildFactoryMethodName,
                                                    childTypeQualifiedName,
                                                    childExternalDependencyImplementationTypeQualifiedName,
                                                    SpecContainerCollectionReferenceName,
                                                    factory.Location);
                                        }))
                        .ToImmutableList();

                return new InjectorTemplate(
                        injectorDefinition.InjectorType.TypeName,
                        injectorDefinition.InjectorInterfaceType.QualifiedName,
                        specContainerCollectionDeclaration,
                        specContainerCollectionReferenceDeclaration,
                        injectorConstructor,
                        injectorMemberTemplates,
                        injectorDefinition.Location);
            }
        }
    }
}
