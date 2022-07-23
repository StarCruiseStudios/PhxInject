// -----------------------------------------------------------------------------
//  <copyright file="InjectorMethodTemplateBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Construct {
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Render.Templates;

    internal class InjectorMethodTemplateBuilder : ITemplateBuilder<InjectorMethodDefinition, InjectorMethodTemplate> {
        private readonly ITemplateBuilder<FactoryMethodContainerInvocationDefinition, FactoryMethodContainerInvocationTemplate> factoryMethodContainerInvocationBuilder;

        public InjectorMethodTemplateBuilder(
            ITemplateBuilder<FactoryMethodContainerInvocationDefinition, FactoryMethodContainerInvocationTemplate> factoryMethodContainerInvocationBuilder
        ) {
            this.factoryMethodContainerInvocationBuilder = factoryMethodContainerInvocationBuilder;
        }

        public InjectorMethodTemplate Build(InjectorMethodDefinition definition) {
            return new InjectorMethodTemplate(
                definition.ReturnType.QualifiedName,
                definition.InjectorMethodName,
                factoryMethodContainerInvocationBuilder.Build(definition.FactoryMethodContainerInvocation));
        }
    }
}
