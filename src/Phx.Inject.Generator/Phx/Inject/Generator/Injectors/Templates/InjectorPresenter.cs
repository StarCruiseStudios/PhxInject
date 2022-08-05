// -----------------------------------------------------------------------------
//  <copyright file="InjectorPresenter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Templates {
    using Phx.Inject.Generator.Common.Templates;
    using Phx.Inject.Generator.Injectors.Definitions;

    internal class InjectorPresenter {
        private readonly CreateInjectorTemplate createInjectorTemplate;

        public InjectorPresenter(CreateInjectorTemplate createInjectorTemplate) {
            this.createInjectorTemplate = createInjectorTemplate;
        }

        public InjectorPresenter() : this(new InjectorTemplate.Builder().Build) { }

        public IRenderTemplate Generate(InjectorDefinition injectorDefinition, TemplateGenerationContext context) {
            return new GeneratedFileTemplate(
                    injectorDefinition.InjectorType.NamespaceName,
                    createInjectorTemplate(injectorDefinition, context),
                    injectorDefinition.Location);
        }
    }
}
