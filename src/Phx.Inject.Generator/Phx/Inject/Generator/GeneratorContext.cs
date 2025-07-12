// -----------------------------------------------------------------------------
// <copyright file="GeneratorContext.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator;

internal interface IGeneratorContext {
    string? Description { get; }
    ISymbol? Symbol { get; }
    IExceptionAggregator Aggregator { get; }
    IGeneratorContext? ParentContext { get; }
    GeneratorExecutionContext ExecutionContext { get; }
    int ContextDepth { get; }
}

internal static class IGeneratorContextExtensions {
    public static IInjectionFrame GetFrame(this IGeneratorContext currentCtx) {
        return new InjectionFrame(
            currentCtx.Description,
            currentCtx.Symbol,
            currentCtx.ParentContext?.GetFrame());
    }

    public static Location GetLocation(this IGeneratorContext currentCtx) {
        return currentCtx.Symbol.GetLocationOrDefault();
    }
}

internal class GeneratorContext : IGeneratorContext {
    private IExceptionAggregator? aggregator;

    private GeneratorContext(GeneratorExecutionContext executionContext) {
        ExecutionContext = executionContext;
    }
    public string Description { get => $"generating injection source for {ExecutionContext.Compilation.AssemblyName}"; }
    public ISymbol? Symbol { get => null; }
    public IExceptionAggregator Aggregator { get => aggregator!; }

    public IGeneratorContext? ParentContext { get => null; }

    public GeneratorExecutionContext ExecutionContext { get; }
    public int ContextDepth { get => 0; }

    public static void UseContext(GeneratorExecutionContext executionContext, Action<GeneratorContext> action) {
        var newCtx = new GeneratorContext(executionContext);
        newCtx.Log(newCtx.Description, Location.None);
        try {
            ExceptionAggregator.Try<object?>(
                newCtx.Description,
                newCtx,
                exceptionAggregator => {
                    newCtx.aggregator = exceptionAggregator;
                    action(newCtx);
                    return null;
                });
        } catch (InjectionException) {
            // Ignore injection exceptions to allow partial generation to complete.
            // They are already reported as diagnostics.
        }
    }
}
