// -----------------------------------------------------------------------------
//  <copyright file="IRenderTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Generator.Project.Templates;

internal interface IRenderTemplate : ISourceCodeElement {
    void Render(IRenderWriter writer, RenderContext renderCtx);
}

internal record RenderContext : IGeneratorContext {
    public GeneratorSettings GeneratorSettings { get; }
    public ISymbol? Symbol { get; private init; }
    public IGeneratorContext? ParentContext { get; }
    public GeneratorExecutionContext ExecutionContext { get; }
    
    public RenderContext(
        GeneratorSettings generatorSettings,
        ISymbol? symbol,
        RenderContext? parentCtx,
        GeneratorExecutionContext executionCtx
    ) {
        GeneratorSettings = generatorSettings;
        Symbol = symbol;
        ParentContext = parentCtx;
        ExecutionContext = executionCtx;
    }
}
