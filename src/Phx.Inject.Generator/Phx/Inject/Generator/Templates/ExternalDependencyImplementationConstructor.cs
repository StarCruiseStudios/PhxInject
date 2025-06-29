// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyImplementationConstructor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using Phx.Inject.Generator.Definitions;

    internal class ExternalDependencyImplementationConstructor {
        private readonly ExternalDependencyImplementationTemplate.IBuilder externalDependencyImplementationTemplateBuilder;

        public ExternalDependencyImplementationConstructor(
            ExternalDependencyImplementationTemplate.IBuilder externalDependencyImplementationTemplateBuilder
        ) {
            this.externalDependencyImplementationTemplateBuilder = externalDependencyImplementationTemplateBuilder;
        }

        public ExternalDependencyImplementationConstructor() : this(
            new ExternalDependencyImplementationTemplate.Builder()) { }

        public IRenderTemplate Construct(
            ExternalDependencyImplementationDefinition externalDependencyImplementationDefinition,
            TemplateGenerationContext context
        ) {
            return new GeneratedFileTemplate(
                externalDependencyImplementationDefinition.ExternalDependencyImplementationType.NamespaceName,
                externalDependencyImplementationTemplateBuilder.Build(externalDependencyImplementationDefinition, context),
                externalDependencyImplementationDefinition.Location);
        }
    }
}
