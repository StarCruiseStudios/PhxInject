// -----------------------------------------------------------------------------
//  <copyright file="TemplateRenderer.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render {
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Phx.Inject.Generator.Model;

    internal class TemplateRenderer {
        private CreateRenderWriter createRenderWriter;

        public TemplateRenderer(CreateRenderWriter createRenderWriter) {
            this.createRenderWriter = createRenderWriter;
        }

        public void RenderTemplate(string fileName, IRenderTemplate template, GeneratorExecutionContext context) {
            var renderWriter = createRenderWriter();
            template.Render(renderWriter);

            var classSource = renderWriter.GetRenderedString();
            if (renderWriter.Settings.ShouldWriteFiles) {
                try {
                    if (!Directory.Exists(renderWriter.Settings.OutputPath)) {
                        Directory.CreateDirectory(renderWriter.Settings.OutputPath);
                    }

                    var outputPath = Path.Join(renderWriter.Settings.OutputPath, fileName);
                    File.WriteAllText(outputPath, classSource);
                } catch (Exception ex) {
                    Logger.Error(ex.ToString());
                }
            }
            var classSourceText = SourceText.From(classSource, Encoding.UTF8);

            context.AddSource(fileName, classSourceText);
        }
    }
}
