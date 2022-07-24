// -----------------------------------------------------------------------------
//  <copyright file="InjectorTemplateBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Construct {
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Render.Templates;

    internal class InjectorTemplateBuilder : IFileTemplateBuilder<InjectorDefinition> {
        private readonly ITemplateBuilder<InjectorMethodDefinition, InjectorMethodTemplate> injectorMethodBuilder;
        private readonly ITemplateBuilder<InjectorBuilderMethodDefinition, InjectorBuilderMethodTemplate> injectorBuilderMethodBuilder;

        public InjectorTemplateBuilder(
                ITemplateBuilder<InjectorMethodDefinition, InjectorMethodTemplate> injectorMethodBuilder,
                ITemplateBuilder<InjectorBuilderMethodDefinition, InjectorBuilderMethodTemplate> injectorBuilderMethodBuilder) {
            this.injectorMethodBuilder = injectorMethodBuilder;
            this.injectorBuilderMethodBuilder = injectorBuilderMethodBuilder;
        }

        public GeneratedFileTemplate Build(InjectorDefinition definition) {
            var injectorMethods = definition.InjectorMethods
                .Select(injectorMethodBuilder.Build);

            var injectorBuilderMethods = definition.InjectorBuilderMethods
                .Select(injectorBuilderMethodBuilder.Build);
            
            var specContainerCollectionInterfaceTemplate = new SpecContainerCollectionInterfaceTemplate(
                definition.SpecContainerTypes.Select(specContainer => new SpecContainerPropertyDeclarationTemplate(specContainer.NamespaceName, specContainer.Name)));
            var specContainerCollectionImplementationTemplate = new SpecContainerCollectionImplementationTemplate(
                definition.SpecContainerTypes.Select(specContainer => new SpecContainerPropertyDefinitionTemplate(specContainer.NamespaceName, specContainer.Name)));

            return new GeneratedFileTemplate(definition.InjectorType.NamespaceName,
                new InjectorTemplate(
                    InjectorClassName: definition.InjectorType.Name,
                    InjectorInterfaceQualifiedName: definition.InjectorInterfaceType.QualifiedName,
                    SpecContainerCollectionInterfaceTemplate: specContainerCollectionInterfaceTemplate,
                    SpecContainerCollectionImplementationTemplate: specContainerCollectionImplementationTemplate,
                    InjectorMethods: injectorMethods,
                    InjectorBuilderMethods: injectorBuilderMethods));
        }
    }
}
