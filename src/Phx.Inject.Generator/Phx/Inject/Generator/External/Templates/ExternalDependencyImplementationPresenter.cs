﻿// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyImplementationPresenter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Presenter {
    using Phx.Inject.Generator.Model;
    using Phx.Inject.Generator.Model.External.Definitions;
    using Phx.Inject.Generator.Model.External.Templates;

    internal class ExternalDependencyImplementationPresenter {
        private readonly CreateExternalDependencyImplementationTemplate createExternalDependencyImplementationTemplate;

        public ExternalDependencyImplementationPresenter(
                CreateExternalDependencyImplementationTemplate createExternalDependencyImplementationTemplate
        ) {
            this.createExternalDependencyImplementationTemplate = createExternalDependencyImplementationTemplate;
        }

        public ExternalDependencyImplementationPresenter() : this(
                new ExternalDependencyImplementationTemplate.Builder().Build) { }

        public IRenderTemplate Generate(
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