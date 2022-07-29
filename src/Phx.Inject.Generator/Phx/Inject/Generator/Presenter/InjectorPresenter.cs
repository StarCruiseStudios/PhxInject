// -----------------------------------------------------------------------------
//  <copyright file="InjectorPresenter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Presenter {
    using Phx.Inject.Generator.Model.Definitions;
    using Phx.Inject.Generator.Model.Templates;

    internal class InjectorPresenter {
        public InjectorPresenter(
                InjectorDefinition injectorDefinition,
                CreateSpecContainerFactoryMethodInvocationTemplate createSpecContainerFactoryMethodInvocationTemplate
        ) {
            // var specContainerFactoryInvocationTemplateBuilder
            //         = new SpecContainerFactoryMethodInvocationTemplate.Builder();

            var injectorTemplateBuilder = new InjectorTemplate.Builder(
                    new SpecContainerCollectionTemplate.Builder(
                            new SpecContainerCollectionPropertyDefinitionTemplate.Builder().Build
                    ).Build,
                    new InjectorProviderMethodTemplate.Builder(
                            createSpecContainerFactoryMethodInvocationTemplate
                    ).Build,
                    new InjectorBuilderMethodTemplate.Builder(
                            new SpecContainerBuilderMethodInvocationTemplate.Builder().Build
                    ).Build
            );

            new GeneratedFileTemplate(
                    injectorDefinition.InjectorType.NamespaceName,
                    injectorTemplateBuilder.Build(injectorDefinition),
                    injectorDefinition.Location
            );
        }
    }
}
