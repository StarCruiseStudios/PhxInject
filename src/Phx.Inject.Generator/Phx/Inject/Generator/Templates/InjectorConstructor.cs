// -----------------------------------------------------------------------------
//  <copyright file="InjectorConstructor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using Phx.Inject.Generator.Definitions;

    internal class InjectorConstructor {
        private readonly InjectorTemplate.IBuilder injectorTemplateBuilder;

        public InjectorConstructor(InjectorTemplate.IBuilder injectorTemplateBuilder) {
            this.injectorTemplateBuilder = injectorTemplateBuilder;
        }

        public InjectorConstructor() : this(new InjectorTemplate.Builder()) { }

        public IRenderTemplate Construct(InjectorDefinition injectorDefinition, TemplateGenerationContext context) {
            return new GeneratedFileTemplate(
                injectorDefinition.InjectorType.NamespaceName,
                injectorTemplateBuilder.Build(injectorDefinition, context),
                injectorDefinition.Location);
        }
    }
}
