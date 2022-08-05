// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyImplementationConstructor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.External.Templates {
    using Phx.Inject.Generator.Common.Templates;
    using Phx.Inject.Generator.External.Definitions;

    internal class ExternalDependencyImplementationConstructor {
        private readonly CreateExternalDependencyImplementationTemplate createExternalDependencyImplementationTemplate;

        public ExternalDependencyImplementationConstructor(
                CreateExternalDependencyImplementationTemplate createExternalDependencyImplementationTemplate
        ) {
            this.createExternalDependencyImplementationTemplate = createExternalDependencyImplementationTemplate;
        }

        public ExternalDependencyImplementationConstructor() : this(
                new ExternalDependencyImplementationTemplate.Builder().Build) { }

        public IRenderTemplate Construct(
                ExternalDependencyImplementationDefinition externalDependencyImplementationDefinition,
                TemplateGenerationContext context
        ) {
            return new GeneratedFileTemplate(
                    externalDependencyImplementationDefinition.ExternalDependencyImplementationType.NamespaceName,
                    createExternalDependencyImplementationTemplate(externalDependencyImplementationDefinition, context),
                    externalDependencyImplementationDefinition.Location);
        }
    }
}
