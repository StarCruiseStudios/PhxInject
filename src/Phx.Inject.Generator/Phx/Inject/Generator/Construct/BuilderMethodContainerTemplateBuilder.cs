// -----------------------------------------------------------------------------
//  <copyright file="BuilderMethodContainerTemplateBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Construct {
    using System.Collections.Immutable;
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Render.Templates;

    internal class BuilderMethodContainerTemplateBuilder : ITemplateBuilder<BuilderMethodContainerDefinition, BuilderMethodContainerTemplate> {
        private readonly ITemplateBuilder<FactoryMethodContainerInvocationDefinition, FactoryMethodContainerInvocationTemplate> factoryMethodContainerInvocationBuilder;

        public BuilderMethodContainerTemplateBuilder(
            ITemplateBuilder<FactoryMethodContainerInvocationDefinition, FactoryMethodContainerInvocationTemplate> factoryMethodContainerInvocationBuilder
        ) {
            this.factoryMethodContainerInvocationBuilder = factoryMethodContainerInvocationBuilder;
        }

        public BuilderMethodContainerTemplate Build(BuilderMethodContainerDefinition definition) {
            var arguments = definition.Arguments
                .Select(factoryMethodContainerInvocationBuilder.Build)
                .ToImmutableList();

            return new BuilderMethodContainerTemplate(
                BuiltTypeQualifiedName: definition.BuiltType.QualifiedName,
                BuilderMethodName: definition.BuilderMethodName,
                SpecContainerCollectionQualifiedName: definition.SpecContainerCollectionType.QualifiedName,
                SpecificationQualifiedName: definition.SpecType.QualifiedName,
                Arguments: arguments
            );
        }
    }
}