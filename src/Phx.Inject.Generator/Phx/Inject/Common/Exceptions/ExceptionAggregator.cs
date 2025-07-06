// -----------------------------------------------------------------------------
// <copyright file="ExceptionAggregator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator;

namespace Phx.Inject.Common.Exceptions;

internal interface IExceptionAggregator {
    T Aggregate<T>(string message, Func<T> func, Func<Exception, T> defaultValue);
    IReadOnlyList<R> AggregateMany<T, R>(IEnumerable<T> elements, Func<T, string> elementDescription, Func<T, R> func);
    void Aggregate(string message, Action action);
}

internal sealed class ExceptionAggregator : IExceptionAggregator {
    private readonly List<InjectionException> exceptions = new();
    private readonly IGeneratorContext generatorContext;
    private readonly string message;

    private ExceptionAggregator(
        string message,
        IGeneratorContext generatorContext
    ) {
        this.message = message;
        this.generatorContext = generatorContext;
    }

    public T Aggregate<T>(string message, Func<T> func, Func<Exception, T> defaultValue) {
        try {
            return func();
        } catch (Exception e) {
            var ie = e.AsInjectionException(message, generatorContext);
            exceptions.Add(ie);
            return defaultValue(ie);
        }
    }

    public void Aggregate(string message, Action action) {
        try {
            action();
        } catch (Exception e) {
            exceptions.Add(e.AsInjectionException(message, generatorContext));
        }
    }

    public IReadOnlyList<R> AggregateMany<T, R>(
        IEnumerable<T> elements,
        Func<T, string> elementDescription,
        Func<T, R> func
    ) {
        var results = new List<R>();
        foreach (var element in elements) {
            try {
                results.Add(func(element));
            } catch (Exception e) {
                exceptions.Add(e.AsInjectionException(elementDescription(element), generatorContext));
            }
        }

        return results;
    }

    private T Throw<T>(IGeneratorContext generatorContext) {
        throw Diagnostics.AggregateError.AsAggregateException(message, exceptions, generatorContext);
    }

    public static T Try<T>(
        string description,
        IGeneratorContext generatorCtx,
        Func<IExceptionAggregator, T> func
    ) {
        var aggregateException = new ExceptionAggregator(
            $"One or more errors occurred while {description}.",
            generatorCtx);
        var result =
            aggregateException.Aggregate(
                description,
                () => func(aggregateException),
                e => aggregateException.Throw<T>(generatorCtx));

        return aggregateException.exceptions.OfType<FatalInjectionException>().Any()
            ? aggregateException.Throw<T>(generatorCtx)
            : result;
    }

    public static void Try(
        string description,
        IGeneratorContext generatorCtx,
        params Action<IExceptionAggregator>[] actions
    ) {
        var aggregator = new ExceptionAggregator(
            $"One or more errors error occurred while {description}.",
            generatorCtx);
        foreach (var action in actions) {
            aggregator.Aggregate(description, () => action(aggregator));
        }
        
        if (aggregator.exceptions.Count > 0) {
            aggregator.Throw<object>(generatorCtx);
        }
    }
}

internal static class ExceptionAggregatorExtensions {
    public static IEnumerable<TResult> SelectCatching<TSource, TResult>(
        this IEnumerable<TSource> source,
        IExceptionAggregator aggregator,
        Func<TSource, string> elementDescription,
        Func<TSource, TResult> selector
    ) {
        return aggregator.AggregateMany(source, elementDescription, selector);
    }

    public static InjectionException AsInjectionException(
        this Exception e,
        string message,
        IGeneratorContext generatorContext
    ) {
        return e as InjectionException
            ?? Diagnostics.UnexpectedError.AsFatalException(
                $"Unexpected error while {message}: {e}",
                generatorContext.GetLocation(),
                generatorContext);
    }
}
