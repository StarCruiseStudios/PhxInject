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
    using Phx.Inject.Generator.Render.Templates;
 
    internal class TemplateRenderer : ITemplateRenderer {
        private readonly Func<IRenderWriter> newRenderWriter;

        public TemplateRenderer(Func<IRenderWriter> renderWriterFactory) {
            newRenderWriter = renderWriterFactory;
        }
        public void RenderTemplate(
            string fileName,
            IRenderTemplate template,
            GeneratorExecutionContext context
        ) {
            var renderWriter = newRenderWriter();
            template.Render(renderWriter);

            var classSource = renderWriter.GetRenderedString();
            var classSourceText = SourceText.From(classSource, Encoding.UTF8);

            context.AddSource(fileName, classSourceText);
        }
    }
}
