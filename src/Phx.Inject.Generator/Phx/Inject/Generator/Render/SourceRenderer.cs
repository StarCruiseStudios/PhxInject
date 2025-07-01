// -----------------------------------------------------------------------------
// <copyright file="SourceRenderer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Render;

internal class SourceRenderer {
    private readonly GeneratorSettings generatorSettings;

    public SourceRenderer(GeneratorSettings generatorSettings) {
        this.generatorSettings = generatorSettings;
    }

    public void RenderAllTemplates(
        IReadOnlyList<(TypeModel, IRenderTemplate)> templates,
        GeneratorExecutionContext context) {
        var templateRenderer = new TemplateRenderer(new RenderWriter.Factory(generatorSettings));
        InjectionException.Try(() => {
            templates.SelectCatching(t => {
                var (classType, template) = t;
                var fileName = $"{classType.QualifiedName}.{generatorSettings.GeneratedFileExtension}";
                Logger.Info($"Rendering source for {fileName}");
                templateRenderer.RenderTemplate(fileName, template, context);
                return fileName;
            });
        }, "rendering source templates");
    }
}
