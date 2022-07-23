// -----------------------------------------------------------------------------
//  <copyright file="FactoryMethodContainerTemplateBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Construct {
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Render.Templates;

    internal class FactoryMethodContainerTemplateBuilder : ITemplateBuilder<FactoryMethodContainerDefinition, FactoryMethodContainerTemplate> {
        private readonly ITemplateBuilder<FactoryMethodContainerInvocationDefinition, FactoryMethodContainerInvocationTemplate> factoryMethodContainerInvocationBuilder;

        public FactoryMethodContainerTemplateBuilder(
            ITemplateBuilder<FactoryMethodContainerInvocationDefinition, FactoryMethodContainerInvocationTemplate> factoryMethodContainerInvocationBuilder
        ) {
            this.factoryMethodContainerInvocationBuilder = factoryMethodContainerInvocationBuilder;
        }

        public FactoryMethodContainerTemplate Build(FactoryMethodContainerDefinition definition) {
            var arguments = definition.Arguments
                .Select(factoryMethodContainerInvocationBuilder.Build);

            return new FactoryMethodContainerTemplate(
                ReturnTypeQualifiedName: definition.ReturnType.QualifiedName,
                FactoryMethodName: definition.FactoryMethodName,
                SpecContainerCollectionQualifiedName: definition.SpecContainerCollectionType.QualifiedName,
                InstanceHolderReference: definition.InstanceHolder?.ReferenceName,
                SpecificationQualifiedName: definition.SpecType.QualifiedName,
                Arguments: arguments
            );
        }
    }
}