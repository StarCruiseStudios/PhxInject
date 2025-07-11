// -----------------------------------------------------------------------------
// <copyright file="GeneratorContext.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator;

internal interface IGeneratorContext {
    ISymbol? Symbol { get; }
    IExceptionAggregator Aggregator { get; }
    IGeneratorContext? ParentContext { get; }
    GeneratorExecutionContext ExecutionContext { get; }
}

internal static class IGeneratorContextExtensions {
    public static IInjectionFrame GetFrame(this IGeneratorContext generatorCtx) {
        return new InjectionFrame(generatorCtx.Symbol, generatorCtx.ParentContext?.GetFrame());
    }

    public static Location GetLocation(this IGeneratorContext generatorCtx) {
        return generatorCtx.Symbol?.Locations.First() ?? Location.None;
    }
}

internal class GeneratorContext : IGeneratorContext {
    public ISymbol? Symbol { get => null; }
    
    private IExceptionAggregator? aggregator;
    public IExceptionAggregator Aggregator { get => aggregator!; }
    
    public IGeneratorContext? ParentContext { get => null; }
    
    public GeneratorExecutionContext ExecutionContext { get; }
    
    private GeneratorContext(GeneratorExecutionContext executionContext) {
        ExecutionContext = executionContext;
    }
    
    public static void UseContext(GeneratorExecutionContext executionContext, Action<GeneratorContext> action) {
        var newContext = new GeneratorContext(executionContext);
        try {
            ExceptionAggregator.Try<object?>(
                $"generating injection source for {executionContext.Compilation.AssemblyName}",
                newContext,
                exceptionAggregator => {
                    newContext.aggregator = exceptionAggregator;
                    action(newContext);
                    return null;
                });
        } catch (InjectionException) {
            // Ignore injection exceptions to allow partial generation to complete.
            // They are already reported as diagnostics.
        }
    } 
}
