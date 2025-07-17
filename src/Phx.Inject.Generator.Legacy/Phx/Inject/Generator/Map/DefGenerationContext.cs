// -----------------------------------------------------------------------------
//  <copyright file="DefGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Map;

internal record DefGenerationContext(
    IGeneratorContext ParentContext
) : IGeneratorContext {
    public GeneratorSettings GeneratorSettings { get; } = ParentContext.GeneratorSettings;
    public string Description { get; } = "def generation";
    public ISymbol Symbol { get; private init; } = ParentContext.ExecutionContext.Compilation.Assembly;
    public IExceptionAggregator Aggregator { get; set; } = ParentContext.Aggregator;
    public GeneratorExecutionContext ExecutionContext { get; } = ParentContext.ExecutionContext;
    public int ContextDepth { get; } = ParentContext.ContextDepth + 1;

    public DefGenerationContext GetChildContext(ISymbol symbol) {
        return new DefGenerationContext(
            this
        ) {
            Symbol = symbol
        };
    }
}
