// -----------------------------------------------------------------------------
// <copyright file="IRenderTemplate.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Project.Templates;

internal interface IRenderTemplate : ISourceCodeElement {
    void Render(IRenderWriter writer, RenderContext renderCtx);
}

internal record RenderContext(
    GeneratorSettings GeneratorSettings,
    ISymbol Symbol,
    IGeneratorContext ParentContext
) : IGeneratorContext {
    public string Description { get; } = "Rendering";
    public IExceptionAggregator Aggregator { get; set; } = ParentContext.Aggregator;
    public GeneratorExecutionContext ExecutionContext { get; } = ParentContext.ExecutionContext;
    public int ContextDepth { get; } = ParentContext.ContextDepth + 1;
}
