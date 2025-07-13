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

internal record ExtractorContext(
    string Description,
    ISymbol Symbol,
    IGeneratorContext ParentContext
) : IGeneratorContext {
    public GeneratorSettings GeneratorSettings { get; } = ParentContext.GeneratorSettings;
    public IExceptionAggregator Aggregator { get; set; } = ParentContext.Aggregator;
    public GeneratorExecutionContext ExecutionContext { get; } = ParentContext.ExecutionContext;
    public int ContextDepth { get; } = ParentContext.ContextDepth + 1;
}

internal static class ExtractorContextExtensions {
    public static T UseChildExtractorContext<T>(
        this IGeneratorContext parentCtx,
        string description,
        ISymbol symbol,
        Func<ExtractorContext, T> func
    ) {
        var childCtx = new ExtractorContext(description, symbol, parentCtx);
        var message =
            $"{(childCtx.ContextDepth > 0 ? "|" : "")}{new string(' ', childCtx.ContextDepth * 2)}{description}";
        childCtx.Log(message, Location.None);
        return ExceptionAggregator.Try(
            childCtx.Description,
            childCtx,
            exceptionAggregator => {
                childCtx.Aggregator = exceptionAggregator;
                return func(childCtx);
            });
    }
}
