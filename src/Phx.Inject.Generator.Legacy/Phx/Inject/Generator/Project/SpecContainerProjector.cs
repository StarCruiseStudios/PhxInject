// -----------------------------------------------------------------------------
// <copyright file="SpecContainerProjector.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Project;

internal class SpecContainerProjector {
    private readonly SpecContainerTemplate.IProjector specContainerTemplateProjector;

    public SpecContainerProjector(SpecContainerTemplate.IProjector specContainerTemplateProjector) {
        this.specContainerTemplateProjector = specContainerTemplateProjector;
    }

    public SpecContainerProjector() : this(new SpecContainerTemplate.Projector()) { }

    public IRenderTemplate Project(
        SpecContainerDef specContainerDef,
        TemplateGenerationContext context
    ) {
        return new GeneratedFileTemplate(
            specContainerDef.SpecContainerType.NamespaceName,
            specContainerTemplateProjector.Project(specContainerDef, context),
            specContainerDef.Location);
    }
}
