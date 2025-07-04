// -----------------------------------------------------------------------------
// <copyright file="ExceptionAggregator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common.Exceptions;

internal interface IExceptionAggregator {
    T Aggregate<T>(string message, Func<T> func, Func<T> defaultValue);
    IReadOnlyList<R> AggregateMany<T, R>(IEnumerable<T> elements, Func<T, string> elementDescription, Func<T, R> func);
    void Aggregate(string message, Action action);
}

internal sealed class ExceptionAggregator : IExceptionAggregator {
    private readonly List<InjectionException> exceptions = new();
    private readonly GeneratorExecutionContext generatorContext;
    private readonly Location location;
    private readonly string message;

    private ExceptionAggregator(
        string message,
        Location location,
        GeneratorExecutionContext generatorContext
    ) {
        this.message = message;
        this.generatorContext = generatorContext;
        this.location = location;
    }

    public T Aggregate<T>(string message, Func<T> func, Func<T> defaultValue) {
        try {
            return func();
        } catch (Exception e) {
            exceptions.Add(AsInjectionException(message, e));
        }

        return defaultValue();
    }

    public void Aggregate(string message, Action action) {
        try {
            action();
        } catch (Exception e) {
            exceptions.Add(AsInjectionException(message, e));
        }
    }

    public IReadOnlyList<R> AggregateMany<T, R>(IEnumerable<T> elements, Func<T, string> elementDescription, Func<T, R> func) {
        var results = new List<R>();
        foreach (var element in elements) {
            try {
                results.Add(func(element));
            } catch (Exception e) {
                exceptions.Add(AsInjectionException(elementDescription(element), e));
            }
        }

        return results;
    }

    private T Throw<T>() {
        throw new AggregateInjectionException(message, location, exceptions, generatorContext);
    }

    private InjectionException AsInjectionException(string message, Exception e) {
        return e is InjectionException ie
            ? ie
            : Diagnostics.UnexpectedError.AsException(
                $"Unexpected error while {message}: {e}",
                location,
                generatorContext);
    }

    public static T Try<T>(
        string description,
        Location location,
        GeneratorExecutionContext generatorContext,
        Func<IExceptionAggregator, T> func
    ) {
        var aggregateException = new ExceptionAggregator($"One or more errors occurred while {description}.",
            location,
            generatorContext);
        var result =
            aggregateException.Aggregate(description, () => func(aggregateException), aggregateException.Throw<T>);

        if (aggregateException.exceptions.Count > 0) {
            aggregateException.Throw<T>();
        }

        return result;
    }

    public static void Try(
        string description,
        Location location,
        GeneratorExecutionContext generatorContext,
        Action<IExceptionAggregator> action
    ) {
        var aggregateException = new ExceptionAggregator($"One or more errors error occurred while {description}.",
            location,
            generatorContext);
        aggregateException.Aggregate(description, () => action(aggregateException));

        if (aggregateException.exceptions.Count > 0) {
            aggregateException.Throw<object>();
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
}
