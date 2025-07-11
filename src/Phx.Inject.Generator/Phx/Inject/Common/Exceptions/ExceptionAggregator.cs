﻿// -----------------------------------------------------------------------------
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
    void Aggregate(string message, params Action[] actions);
    IReadOnlyList<R> AggregateMany<T, R>(IEnumerable<T> elements, Func<T, string> elementDescription, Func<T, R> func);
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

    public void Aggregate(string message, params Action[] actions) {
        foreach (var action in actions) {
            try {
                action();
            } catch (Exception e) {
                exceptions.Add(AsInjectionException(e, message, generatorContext));
            }
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
                exceptions.Add(AsInjectionException(e, elementDescription(element), generatorContext));
            }
        }

        return results;
    }

    private AggregateInjectionException AsException() {
        return Diagnostics.AggregateError.AsAggregateException(message, exceptions, generatorContext);
    }
    
    private T Throw<T>() {
        throw AsException();
    }

    public static T Try<T>(
        string description,
        IGeneratorContext generatorCtx,
        Func<IExceptionAggregator, T> func
    ) {
        var aggregator = new ExceptionAggregator(
            $"One or more errors occurred while {description}.",
            generatorCtx);

        try {
            var result = func(aggregator);
            return aggregator.exceptions.OfType<FatalInjectionException>().Any()
                ? aggregator.Throw<T>()
                : result;
        } catch (Exception e) {
            var ie = AsInjectionException(e, description, generatorCtx);
            aggregator.exceptions.Add(ie);
            throw aggregator.AsException();
        }
    }

    private static InjectionException AsInjectionException(
        Exception e,
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
