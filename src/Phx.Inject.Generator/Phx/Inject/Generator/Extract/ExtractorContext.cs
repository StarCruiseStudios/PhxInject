// -----------------------------------------------------------------------------
//  <copyright file="ExtractionContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Extract;

internal class ExtractorContext : IGeneratorContext {
    public ExtractorContext(
        string? description,
        ISymbol? symbol,
        IGeneratorContext parentContext
    ) {
        Description = description;
        Symbol = symbol;
        Aggregator = parentContext.Aggregator;
        ParentContext = parentContext;
        ExecutionContext = parentContext.ExecutionContext;
        ContextDepth = parentContext.ContextDepth + 1;
    }
    public string? Description { get; }
    public ISymbol? Symbol { get; }
    public IExceptionAggregator Aggregator { get; set; }
    public IGeneratorContext? ParentContext { get; }
    public GeneratorExecutionContext ExecutionContext { get; }
    public int ContextDepth { get; }

    public T UseChildContext<T>(string description, ISymbol symbol, Func<ExtractorContext, T> func) {
        var childContext = new ExtractorContext(description, symbol, this);
        var message =
            $"{(childContext.ContextDepth > 0 ? "|" : "")}{new string(' ', childContext.ContextDepth * 2)}{description}";
        childContext.Log(message, Location.None);
        return ExceptionAggregator.Try(
            $"extracting {symbol}",
            childContext,
            exceptionAggregator => {
                childContext.Aggregator = exceptionAggregator;
                return func(childContext);
            });
    }
}
