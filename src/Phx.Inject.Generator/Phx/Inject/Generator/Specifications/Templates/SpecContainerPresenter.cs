﻿// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerPresenter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Templates {
    using Phx.Inject.Generator.Common.Templates;
    using Phx.Inject.Generator.Specifications.Definitions;

    internal class SpecContainerPresenter {
        private readonly CreateSpecContainerTemplate createSpecContainerTemplate;

        public SpecContainerPresenter(CreateSpecContainerTemplate createSpecContainerTemplate) {
            this.createSpecContainerTemplate = createSpecContainerTemplate;
        }

        public SpecContainerPresenter() : this(new SpecContainerTemplate.Builder().Build) { }

        public IRenderTemplate Generate(
                SpecContainerDefinition specContainerDefinition,
                TemplateGenerationContext context
        ) {
            return new GeneratedFileTemplate(
                    specContainerDefinition.SpecContainerType.NamespaceName,
                    createSpecContainerTemplate(specContainerDefinition, context),
                    specContainerDefinition.Location);
        }
    }
}
