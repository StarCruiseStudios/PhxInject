// -----------------------------------------------------------------------------
//  <copyright file="DependencyImplementationConstructor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using Phx.Inject.Generator.Definitions;

    internal class DependencyImplementationConstructor {
        private readonly DependencyImplementationTemplate.IBuilder dependencyImplementationTemplateBuilder;

        public DependencyImplementationConstructor(
            DependencyImplementationTemplate.IBuilder dependencyImplementationTemplateBuilder
        ) {
            this.dependencyImplementationTemplateBuilder = dependencyImplementationTemplateBuilder;
        }

        public DependencyImplementationConstructor() : this(
            new DependencyImplementationTemplate.Builder()) { }

        public IRenderTemplate Construct(
            DependencyImplementationDef dependencyImplementationDef,
            TemplateGenerationContext context
        ) {
            return new GeneratedFileTemplate(
                dependencyImplementationDef.DependencyImplementationType.NamespaceName,
                dependencyImplementationTemplateBuilder.Build(dependencyImplementationDef, context),
                dependencyImplementationDef.Location);
        }
    }
}
