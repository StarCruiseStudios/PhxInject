// -----------------------------------------------------------------------------
//  <copyright file="TemplateRenderer.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

#undef SHOULD_WRITE_FILES

namespace Phx.Inject.Generator.Common.Render {
#if SHOULD_WRITE_FILES
    using System;
    using System.IO;
#endif
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Phx.Inject.Generator.Common.Templates;

    internal class TemplateRenderer {
        private readonly CreateRenderWriter createRenderWriter;

        public TemplateRenderer(CreateRenderWriter createRenderWriter) {
            this.createRenderWriter = createRenderWriter;
        }

        public void RenderTemplate(string fileName, IRenderTemplate template, GeneratorExecutionContext context) {
            var renderWriter = createRenderWriter();
            template.Render(renderWriter);

            var classSource = renderWriter.GetRenderedString();
#if SHOULD_WRITE_FILES
            if (renderWriter.Settings.ShouldWriteFiles) {
                try {
                    if (!Directory.Exists(renderWriter.Settings.OutputPath)) {
                        Directory.CreateDirectory(renderWriter.Settings.OutputPath);
                    }

                    var outputPath = Path.Combine(renderWriter.Settings.OutputPath, fileName);
                    File.WriteAllText(outputPath, classSource);
                } catch (Exception ex) {
                    Logger.Error(ex.ToString());
                }
            }
#endif

            var classSourceText = SourceText.From(classSource, Encoding.UTF8);

            context.AddSource(fileName, classSourceText);
        }
    }
}
