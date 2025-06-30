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
        private readonly SpecContainerTemplate.IBuilder specContainerTemplateBuilder;

        public SpecContainerConstructor(SpecContainerTemplate.IBuilder specContainerTemplateBuilder) {
            this.specContainerTemplateBuilder = specContainerTemplateBuilder;
        }

        public SpecContainerConstructor() : this(new SpecContainerTemplate.Builder()) { }

        public IRenderTemplate Construct(
            SpecContainerDef specContainerDef,
            TemplateGenerationContext context
        ) {
            return new GeneratedFileTemplate(
                specContainerDef.SpecContainerType.NamespaceName,
                specContainerTemplateBuilder.Build(specContainerDef, context),
                specContainerDef.Location);
        }
    }
}
