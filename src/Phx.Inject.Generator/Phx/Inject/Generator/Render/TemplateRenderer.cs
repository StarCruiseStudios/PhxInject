// -----------------------------------------------------------------------------
//  <copyright file="TemplateRenderer.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render {
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Phx.Inject.Generator.Templates;

    internal class TemplateRenderer {
        private readonly IRenderWriterFactory renderWriterFactory;

        public TemplateRenderer(IRenderWriterFactory renderWriterFactory) {
            this.renderWriterFactory = renderWriterFactory;
        }

        public void RenderTemplate(string fileName, IRenderTemplate template, GeneratorExecutionContext context) {
            var renderWriter = renderWriterFactory.Build();
            template.Render(renderWriter);

            var classSource = renderWriter.GetRenderedString();
            var classSourceText = SourceText.From(classSource, Encoding.UTF8);

            context.AddSource(fileName, classSourceText);
        }
    }
}
