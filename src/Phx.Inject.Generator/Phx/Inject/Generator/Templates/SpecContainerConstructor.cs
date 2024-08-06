// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerConstructor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using Phx.Inject.Generator.Definitions;

    internal class SpecContainerConstructor {
        private readonly CreateSpecContainerTemplate createSpecContainerTemplate;

        public SpecContainerConstructor(CreateSpecContainerTemplate createSpecContainerTemplate) {
            this.createSpecContainerTemplate = createSpecContainerTemplate;
        }

        public SpecContainerConstructor() : this(new SpecContainerTemplate.Builder().Build) { }

        public IRenderTemplate Construct(
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
