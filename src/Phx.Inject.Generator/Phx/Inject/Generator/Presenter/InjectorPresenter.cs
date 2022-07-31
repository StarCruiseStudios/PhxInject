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
        private readonly CreateInjectorTemplate createInjectorTemplate;

        public InjectorPresenter(CreateInjectorTemplate createInjectorTemplate) {
            this.createInjectorTemplate = createInjectorTemplate;
        }

        public InjectorPresenter() : this(
                new InjectorTemplate.Builder(
                        new SpecContainerCollectionTemplate.Builder(
                                new SpecContainerCollectionPropertyDefinitionTemplate.Builder().Build).Build,
                        new InjectorProviderMethodTemplate.Builder(
                                new SpecContainerFactoryMethodInvocationTemplate.Builder().Build).Build,
                        new InjectorBuilderMethodTemplate.Builder(
                                new SpecContainerBuilderMethodInvocationTemplate.Builder().Build).Build).Build) { }

        public IRenderTemplate Generate(InjectorDefinition injectorDefinition) {
            return new GeneratedFileTemplate(
                    injectorDefinition.InjectorType.NamespaceName,
                    createInjectorTemplate(injectorDefinition),
                    injectorDefinition.Location
            );
        }
    }
}
