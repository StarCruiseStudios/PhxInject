// -----------------------------------------------------------------------------
//  <copyright file="InjectionController.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Controller {
    using Phx.Inject.Generator.Model.Definitions;
    using InjectorBuilderMethodDefinition = Phx.Inject.Generator.Model.Definitions.InjectorBuilderMethodDefinition;
    using InjectorDefinition = Phx.Inject.Generator.Model.Definitions.InjectorDefinition;
    using SpecContainerDefinition = Phx.Inject.Generator.Model.Definitions.SpecContainerDefinition;

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
                    SpecContainerCollectionTypeGenerator.CreateSpecContainerCollectionType,
                    specContainerReferenceBuilder.Build);

            var injectorDefinitionBuilder = new InjectorDefinition.Builder(
                    injectorProviderMethodDefinitionBuilder.Build,
                    injectorBuilderMethodDefinitionBuilder.Build,
                    specContainerCollectionDefinitionBuilder.Build);

            var specReferenceDefinitionBuilder = new SpecReferenceDefinition.Builder();

            var specContainerFactoryInstanceHolderDefinitionBuilder
                    = new SpecContainerFactoryInstanceHolderDefinition.Builder(
                            InstanceHolderNameGenerator.CreateInstanceHolderName);

            var specContainerFactoryMethodDefinitionBuilder = new SpecContainerFactoryMethodDefinition.Builder(
                    specReferenceDefinitionBuilder.Build,
                    SpecContainerTypeGenerator.CreateSpecContainerType,
                    SpecContainerCollectionTypeGenerator.CreateSpecContainerCollectionType,
                    specContainerFactoryInstanceHolderDefinitionBuilder.Build,
                    specContainerFactoryInvocationDefinitionBuilder.Build);

            var specContainerDefinitionBuilder = new SpecContainerDefinition.Builder(
                    SpecContainerTypeGenerator.CreateSpecContainerType,
                    specReferenceDefinitionBuilder.Build,
                    specContainerFactoryInstanceHolderDefinitionBuilder.Build,
                    specContainerFactoryMethodDefinitionBuilder.Build,
                    null!);

            var injectionContextDefinitionBuilder = new InjectionContextDefinition.Builder(
                    injectorDefinitionBuilder.Build,
                    specContainerDefinitionBuilder.Build);
        }
    }
}
