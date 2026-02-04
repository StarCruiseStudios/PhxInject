// -----------------------------------------------------------------------------
// <copyright file="InjectorProjector.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Map.Definitions;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Project;

internal class InjectorProjector {
    private readonly InjectorTemplate.IProjector injectorTemplateProjector;

    public InjectorProjector(InjectorTemplate.IProjector injectorTemplateProjector) {
        this.injectorTemplateProjector = injectorTemplateProjector;
    }

    public InjectorProjector() : this(new InjectorTemplate.Projector()) { }

    public IRenderTemplate Project(InjectorDef injectorDef, TemplateGenerationContext context) {
        return new GeneratedFileTemplate(
            injectorDef.InjectorType.NamespaceName,
            injectorTemplateProjector.Project(injectorDef, context),
            injectorDef.Location);
    }
}
