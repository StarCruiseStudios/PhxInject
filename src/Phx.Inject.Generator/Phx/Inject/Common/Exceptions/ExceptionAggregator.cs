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
    IReadOnlyList<T> Aggregate<T>(string message, Func<T> func);
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
            exceptions.Add(AsInjectionException(e));
        }

        return defaultValue();
    }

    public IReadOnlyList<T> Aggregate<T>(string message, Func<T> func) {
        try {
            return ImmutableList.Create(func());
        } catch (Exception e) {
            exceptions.Add(AsInjectionException(e));
        }

        return ImmutableList<T>.Empty;
    }

    public void Aggregate(string message, Action action) {
        try {
            action();
        } catch (Exception e) {
            exceptions.Add(AsInjectionException(e));
        }
    }

    private T Throw<T>() {
        throw new AggregateInjectionException(message, location, exceptions, generatorContext);
    }

    private InjectionException AsInjectionException(Exception e) {
        return e is InjectionException ie
            ? ie
            : Diagnostics.UnexpectedError.AsException(
                $"Unexpected error while {message}: {e}",
                location,
                generatorContext);
    }

    public static T Try<T>(
        string description,
        GeneratorExecutionContext generatorContext,
        Func<IExceptionAggregator, T> func
    ) {
        return Try(description, Location.None, generatorContext, func);
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
        GeneratorExecutionContext generatorContext,
        Action<IExceptionAggregator> action
    ) {
        Try(description, Location.None, generatorContext, action);
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
        string description,
        Func<TSource, string> elementDescription,
        GeneratorExecutionContext generatorContext,
        Func<TSource, TResult> selector
    ) {
        return source.SelectCatching(description, elementDescription, Location.None, generatorContext, selector);
    }

    public static IEnumerable<TResult> SelectCatching<TSource, TResult>(
        this IEnumerable<TSource> source,
        string description,
        Func<TSource, string> elementDescription,
        Location location,
        GeneratorExecutionContext generatorContext,
        Func<TSource, TResult> selector
    ) {
        return ExceptionAggregator.Try(
            description,
            location,
            generatorContext,
            aggregator => source.SelectCatching(aggregator, elementDescription, selector));
    }

    public static IEnumerable<TResult> SelectCatching<TSource, TResult>(
        this IEnumerable<TSource> source,
        IExceptionAggregator aggregator,
        Func<TSource, string> elementDescription,
        Func<TSource, TResult> selector
    ) {
        var results = new List<TResult>();
        foreach (var s in source) {
            results.AddRange(aggregator.Aggregate(elementDescription(s), () => selector(s)));
        }

        return results;
    }
}
