// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplateBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Construct {
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Render.Templates;
    
    internal class SpecContainerTemplateBuilder : IFileTemplateBuilder<SpecContainerDefinition> {
        private readonly ITemplateBuilder<InstanceHolderDefinition, InstanceHolderDeclarationTemplate> instanceHolderDeclarationBuilder;
        private readonly ITemplateBuilder<FactoryMethodContainerDefinition, FactoryMethodContainerTemplate> factoryMethodContainerBuilder;

        public SpecContainerTemplateBuilder(
            ITemplateBuilder<InstanceHolderDefinition, InstanceHolderDeclarationTemplate> instanceHolderDeclarationBuilder,
            ITemplateBuilder<FactoryMethodContainerDefinition, FactoryMethodContainerTemplate> factoryMethodContainerBuilder
        ) {
            this.instanceHolderDeclarationBuilder = instanceHolderDeclarationBuilder;
            this.factoryMethodContainerBuilder = factoryMethodContainerBuilder;
        }

        public GeneratedFileTemplate Build(SpecContainerDefinition definition) {
            var instanceHolders = definition.InstanceHolderDeclarations
                .Select(instanceHolderDeclarationBuilder.Build);

            var factoryMethodContainers = definition.FactoryMethodContainers
                .Select(factoryMethodContainerBuilder.Build);

            return new GeneratedFileTemplate(definition.ContainerType.NamespaceName,
                new SpecContainerTemplate(
                    SpecContainerClassName: definition.ContainerType.Name,
                    InstanceHolderDeclarations: instanceHolders,
                    FactoryMethodContainers: factoryMethodContainers));
        }
    }
}