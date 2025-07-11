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
    public ISymbol? Symbol { get; }
    public IGeneratorContext? ParentContext { get; }
    public GeneratorExecutionContext ExecutionContext { get; }
    
    public IExceptionAggregator Aggregator { get; set; }
    
    public ExtractorContext(
        ISymbol? symbol,
        IGeneratorContext parentContext
    ) {
        Symbol = symbol;
        ParentContext = parentContext;
        ExecutionContext = parentContext.ExecutionContext;
        Aggregator = parentContext.Aggregator;
    }

    public T UseChildContext<T>(ISymbol symbol, Func<ExtractorContext, T> func) {
        var childContext = new ExtractorContext(symbol, this);
        return ExceptionAggregator.Try(
            $"extracting {symbol}",
            childContext,
            exceptionAggregator => {
                childContext.Aggregator = exceptionAggregator;
                return func(childContext);
            });
    }
}
