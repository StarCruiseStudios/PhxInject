// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerConstructor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Project;

internal class SpecContainerProjector {
    private readonly SpecContainerTemplate.IBuilder specContainerTemplateBuilder;

    public SpecContainerProjector(SpecContainerTemplate.IBuilder specContainerTemplateBuilder) {
        this.specContainerTemplateBuilder = specContainerTemplateBuilder;
    }

    public SpecContainerProjector() : this(new SpecContainerTemplate.Builder()) { }

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
