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
    private ExtractorContext(
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
        return UseChildContext(description, symbol, this, func);
    }

    public static T CreateExtractorContext<T>(
        IGeneratorContext parentCtx,
        Func<ExtractorContext, T> func
    ) {
        return UseChildContext(null, null, parentCtx, func);
    }

    private static T UseChildContext<T>(
        string? description,
        ISymbol? symbol,
        IGeneratorContext parentCtx,
        Func<ExtractorContext, T> func
    ) {
        var childCtx = new ExtractorContext(description, symbol, parentCtx);
        var message =
            $"{(childCtx.ContextDepth > 0 ? "|" : "")}{new string(' ', childCtx.ContextDepth * 2)}{description}";
        childCtx.Log(message, Location.None);
        return ExceptionAggregator.Try(
            $"extracting {symbol}",
            childCtx,
            exceptionAggregator => {
                childCtx.Aggregator = exceptionAggregator;
                return func(childCtx);
            });
    }
}
