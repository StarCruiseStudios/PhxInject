// -----------------------------------------------------------------------------
//  <copyright file="InjectionController.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Manager {
    using Phx.Inject.Generator.Model;
    using Phx.Inject.Generator.Model.Definitions;
    using Phx.Inject.Generator.Model.External.Definitions;
    using Phx.Inject.Generator.Model.Injectors.Definitions;
    using Phx.Inject.Generator.Model.Specifications.Definitions;

    internal class InjectionController {
        private readonly CreateInjectionContextDefinition createInjectionContextDefinition;

        public InjectionController(CreateInjectionContextDefinition createInjectionContextDefinition) {
            this.createInjectionContextDefinition = createInjectionContextDefinition;
        }

        public InjectionController() : this(
                new InjectionContextDefinition.Builder(
                        new InjectorDefinition.Builder().Build,
                        new SpecContainerDefinition.Builder().Build,
                        new ExternalDependencyImplementationDefinition.Builder().Build).Build) { }

        public InjectionContextDefinition Map(DefinitionGenerationContext context) {
            return createInjectionContextDefinition(context.Injector, context);
        }
    }
}
