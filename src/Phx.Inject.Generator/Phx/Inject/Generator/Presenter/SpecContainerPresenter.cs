// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerPresenter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Presenter {
    using Phx.Inject.Generator.Model.Definitions;
    using Phx.Inject.Generator.Model.Templates;

    internal class SpecContainerPresenter {
        private readonly CreateSpecContainerTemplate createSpecContainerTemplate;

        public SpecContainerPresenter(CreateSpecContainerTemplate createSpecContainerTemplate) {
            this.createSpecContainerTemplate = createSpecContainerTemplate;
        }

        public SpecContainerPresenter() {
            CreateSpecContainerFactoryMethodInvocationTemplate createSpecContainerFactoryInvocation
                    = new SpecContainerFactoryMethodInvocationTemplate.Builder().Build;

            createSpecContainerTemplate = new SpecContainerTemplate.Builder(
                    new InstanceHolderDeclarationTemplate.Builder().Build,
                    new SpecContainerFactoryMethodTemplate.Builder(createSpecContainerFactoryInvocation).Build,
                    new SpecContainerBuilderMethodTemplate.Builder(createSpecContainerFactoryInvocation).Build
            ).Build;
        }

        public IRenderTemplate Generate(SpecContainerDefinition specContainerDefinition) {
            return new GeneratedFileTemplate(
                    specContainerDefinition.ContainerType.NamespaceName,
                    createSpecContainerTemplate(specContainerDefinition),
                    specContainerDefinition.Location
            );
        }
    }
}
