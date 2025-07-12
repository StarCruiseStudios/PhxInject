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
        ParentContext = parentContext;
        ExecutionContext = parentContext.ExecutionContext;
        Aggregator = parentContext.Aggregator;
    }
    public string? Description { get; }
    public ISymbol? Symbol { get; }
    public IGeneratorContext? ParentContext { get; }
    public GeneratorExecutionContext ExecutionContext { get; }
    public IExceptionAggregator Aggregator { get; set; }

    public T UseChildContext<T>(string description, ISymbol symbol, Func<ExtractorContext, T> func) {
        var childContext = new ExtractorContext(description, symbol, this);
        return ExceptionAggregator.Try(
            $"extracting {symbol}",
            childContext,
            exceptionAggregator => {
                childContext.Aggregator = exceptionAggregator;
                return func(childContext);
            });
    }
}
