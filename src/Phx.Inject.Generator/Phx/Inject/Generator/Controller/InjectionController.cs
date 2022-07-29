// -----------------------------------------------------------------------------
//  <copyright file="InjectionController.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Controller {
    using Phx.Inject.Generator.Model.Definitions;

    internal class InjectionController {
        public InjectionController() {

            var specContainerReferenceBuilder = new SpecContainerReferenceDefinition.Builder(
                    SpecContainerTypeGenerator.CreateSpecContainerType);

            var specContainerFactoryInvocationDefinitionBuilder = new SpecContainerFactoryInvocationDefinition.Builder(
                    specContainerReferenceBuilder.Build);

            var specContainerBuilderInvocationDefinitionBuilder = new SpecContainerBuilderInvocationDefinition.Builder(
                    specContainerReferenceBuilder.Build);


            var injectorProviderMethodDefinitionBuilder = new InjectorProviderMethodDefinition.Builder(
                    specContainerFactoryInvocationDefinitionBuilder.Build);

            var injectorBuilderMethodDefinitionBuilder = new InjectorBuilderMethodDefinition.Builder(
                    specContainerBuilderInvocationDefinitionBuilder.Build);

            var specContainerCollectionDefinitionBuilder = new SpecContainerCollectionDefinition.Builder(
                    specContainerReferenceBuilder.Build);

            var injectorDefinitionBuilder = new InjectorDefinition.Builder(
                    injectorProviderMethodDefinitionBuilder.Build,
                    injectorBuilderMethodDefinitionBuilder.Build,
                    specContainerCollectionDefinitionBuilder.Build);

            var injectionContextDefinitionBuilder = new InjectionContextDefinition.Builder(
                    injectorDefinitionBuilder.Build,
                    createSpecContainer: null!);
        }
    }
}
