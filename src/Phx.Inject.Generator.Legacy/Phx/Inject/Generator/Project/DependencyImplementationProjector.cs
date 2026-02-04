// -----------------------------------------------------------------------------
// <copyright file="DependencyImplementationProjector.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Project;

internal class DependencyImplementationProjector {
    private readonly DependencyImplementationTemplate.IProjector dependencyImplementationTemplateProjector;

    public DependencyImplementationProjector(
        DependencyImplementationTemplate.IProjector dependencyImplementationTemplateProjector
    ) {
        this.dependencyImplementationTemplateProjector = dependencyImplementationTemplateProjector;
    }

    public DependencyImplementationProjector() : this(
        new DependencyImplementationTemplate.Projector()) { }

    public IRenderTemplate Construct(
        DependencyImplementationDef dependencyImplementationDef,
        TemplateGenerationContext context
    ) {
        return new GeneratedFileTemplate(
            dependencyImplementationDef.DependencyImplementationType.NamespaceName,
            dependencyImplementationTemplateProjector.Project(dependencyImplementationDef, context),
            dependencyImplementationDef.Location);
    }
}
