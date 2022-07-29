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
        public SpecContainerPresenter(
                SpecContainerDefinition specContainerDefinition,
                CreateSpecContainerFactoryMethodInvocationTemplate createSpecContainerFactoryMethodInvocationTemplate
        ) {
            var specContainerTemplateBuilder = new SpecContainerTemplate.Builder(
                    null!,
                    null!,
                    null!
            );

            new GeneratedFileTemplate(
                    specContainerDefinition.ContainerType.NamespaceName,
                    specContainerTemplateBuilder.Build(specContainerDefinition),
                    specContainerDefinition.Location
            );
        }
    }
}
