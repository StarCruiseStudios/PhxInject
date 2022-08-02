// -----------------------------------------------------------------------------
//  <copyright file="InjectionController.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Manager {
    using Phx.Inject.Generator.Model.Definitions;
    using Phx.Inject.Generator.Model.Descriptors;
    using InjectorBuilderMethodDefinition = Phx.Inject.Generator.Model.Definitions.InjectorBuilderMethodDefinition;
    using InjectorDefinition = Phx.Inject.Generator.Model.Definitions.InjectorDefinition;
    using SpecContainerDefinition = Phx.Inject.Generator.Model.Definitions.SpecContainerDefinition;

    internal class InjectionController {
        private readonly CreateInjectionContextDefinition createInjectionContextDefinition;

        public InjectionController(CreateInjectionContextDefinition createInjectionContextDefinition) {
            this.createInjectionContextDefinition = createInjectionContextDefinition;
        }

        public InjectionController() {
            var specReferenceDefinitionBuilder = new SpecReferenceDefinition.Builder();
            var specContainerReferenceBuilder = new SpecContainerReferenceDefinition.Builder(
                    SpecContainerTypeGenerator.CreateSpecContainerType);

            var specContainerFactoryInvocationDefinitionBuilder = new SpecContainerFactoryInvocationDefinition.Builder(
                    specContainerReferenceBuilder.Build);
            var specContainerFactoryInstanceHolderDefinitionBuilder
                    = new SpecContainerFactoryInstanceHolderDefinition.Builder(
                            InstanceHolderNameGenerator.CreateInstanceHolderName);

            createInjectionContextDefinition = new InjectionContextDefinition.Builder(
                    new InjectorDefinition.Builder(
                            new ExternalDependencyContainerDefinition.Builder(
                                    new ExternalDependencyProviderMethodDefinition.Builder(
                                            new ExternalDependencySpecFactoryInvocationDefinition.Builder().Build
                                    ).Build).Build,
                            new InjectorProviderMethodDefinition.Builder(
                                    specContainerFactoryInvocationDefinitionBuilder.Build
                            ).Build,
                            new InjectorBuilderMethodDefinition.Builder(
                                    new SpecContainerBuilderInvocationDefinition.Builder(
                                            specContainerReferenceBuilder.Build
                                    ).Build
                            ).Build,
                            new SpecContainerCollectionDefinition.Builder(
                                    SpecContainerCollectionTypeGenerator.CreateSpecContainerCollectionType,
                                    specContainerReferenceBuilder.Build
                            ).Build
                    ).Build,
                    new SpecContainerDefinition.Builder(
                            SpecContainerTypeGenerator.CreateSpecContainerType,
                            specReferenceDefinitionBuilder.Build,
                            specContainerFactoryInstanceHolderDefinitionBuilder.Build,
                            new SpecContainerFactoryMethodDefinition.Builder(
                                    specReferenceDefinitionBuilder.Build,
                                    SpecContainerTypeGenerator.CreateSpecContainerType,
                                    SpecContainerCollectionTypeGenerator.CreateSpecContainerCollectionType,
                                    specContainerFactoryInstanceHolderDefinitionBuilder.Build,
                                    specContainerFactoryInvocationDefinitionBuilder.Build
                            ).Build,
                            new SpecContainerBuilderMethodDefinition.Builder(
                                    specReferenceDefinitionBuilder.Build,
                                    SpecContainerTypeGenerator.CreateSpecContainerType,
                                    SpecContainerCollectionTypeGenerator.CreateSpecContainerCollectionType,
                                    specContainerFactoryInvocationDefinitionBuilder.Build
                            ).Build
                    ).Build).Build;
        }

        public InjectionContextDefinition Map(InjectorDescriptor injectorDescriptor) {
            return createInjectionContextDefinition(injectorDescriptor);
        }
    }
}
