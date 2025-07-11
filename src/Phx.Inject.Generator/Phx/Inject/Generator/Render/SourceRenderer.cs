// -----------------------------------------------------------------------------
// <copyright file="SourceRenderer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Project.Templates;

namespace Phx.Inject.Generator.Render;

internal class SourceRenderer {
    private readonly GeneratorSettings generatorSettings;
    private readonly RenderWriter.Factory writerFactory;

    public SourceRenderer(GeneratorSettings generatorSettings, RenderWriter.Factory writerFactory) {
        this.generatorSettings = generatorSettings;
        this.writerFactory = writerFactory;
    }

    public SourceRenderer(GeneratorSettings generatorSettings)
        : this(generatorSettings, new RenderWriter.Factory(generatorSettings)) { }

    public void Render(
        IReadOnlyList<(TypeModel, IRenderTemplate)> templates,
        IGeneratorContext generatorCtx
    ) {
        var renderContext = new RenderContext(generatorSettings,null, generatorCtx);
        templates.SelectCatching(generatorCtx.Aggregator,
            t => t.ToString(),
            t => {
                var (classType, template) = t;
                var fileName = $"{classType.NamespacedName}.{generatorSettings.GeneratedFileExtension}";
                generatorCtx.Log($"Rendering source for {fileName}");
                generatorCtx.ExecutionContext.AddSource(fileName,
                    writerFactory.Use(writer => template.Render(writer, renderContext)));
                return fileName;
            });
    }
}
