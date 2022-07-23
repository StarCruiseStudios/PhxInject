// -----------------------------------------------------------------------------
//  <copyright file="ITemplateRenderer.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Render.Templates;

    internal interface ITemplateRenderer {
        void RenderTemplate(
            string fileName,
            IRenderTemplate template,
            GeneratorExecutionContext context);
    }
}