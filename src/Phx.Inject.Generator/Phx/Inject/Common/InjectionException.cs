// -----------------------------------------------------------------------------
//  <copyright file="InjectionException.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common;

internal class InjectionException : Exception {
    public Diagnostics.DiagnosticData DiagnosticData { get; }
    public Location Location { get; }
    public List<Diagnostic> Diagnostics { get; } = new();

    public InjectionException(
        GeneratorExecutionContext context,
        IEnumerable<Exception> aggregateExceptions
    ) : base(Common.Diagnostics.AggregateError.Title) {
        DiagnosticData = Common.Diagnostics.AggregateError;
        Location = Location.None;

        foreach (var e in aggregateExceptions) {
            ReportUnexpectedError(context, e);
            Diagnostics.AddRange(GetDiagnosticsFromException(e));
        }
    }
    
    public InjectionException(
        GeneratorExecutionContext context,
        Diagnostics.DiagnosticData diagnosticData,
        string message,
        Location location
    ) : base(message) {
        DiagnosticData = diagnosticData;
        Location = location;
        context.ReportDiagnostic(diagnosticData.CreateDiagnostic(message, location));
        Diagnostics.Add(diagnosticData.CreateDiagnostic(message, location));
    }

    public InjectionException(
        GeneratorExecutionContext context,
        Diagnostics.DiagnosticData? diagnosticData,
        string message,
        Location location,
        Exception inner
    ) : base(message, inner) {
        DiagnosticData = diagnosticData 
            ?? (inner is InjectionException ie && ie.DiagnosticData.Id != Common.Diagnostics.AggregateError.Id
                ? ie.DiagnosticData
                : Common.Diagnostics.UnexpectedError);
        Location = location;
        context.ReportDiagnostic(DiagnosticData.CreateDiagnostic(message, location));
        ReportUnexpectedError(context, inner);
        Diagnostics.Add(DiagnosticData.CreateDiagnostic(message, location));
        Diagnostics.AddRange(GetDiagnosticsFromException(inner));
    }
    
    public T Aggregate<T>(GeneratorExecutionContext context, Func<T> func, Func<T> defaultValue) {
        try {
            return func();
        } catch (Exception e) {
            ReportUnexpectedError(context, e);
            Diagnostics.AddRange(GetDiagnosticsFromException(e));
        }

        return defaultValue();
    }
    
    public void Aggregate(GeneratorExecutionContext context, Action action) {
        try {
            action();
        } catch (Exception e) {
            ReportUnexpectedError(context, e);
            Diagnostics.AddRange(GetDiagnosticsFromException(e));
        }
    }
    
    public void ThrowIfAggregate() {
        if (Diagnostics.Count > 0) {
            throw this;
        }
    }
    
    public static T Try<T>(GeneratorExecutionContext context, Func<T> func, string description) => Try(context, func, description, Location.None);
    
    public static T Try<T>(GeneratorExecutionContext context, Func<T> func, string description, Location location) {
        try {
            return func();
        } catch (Exception e) {
            throw new InjectionException(context, (e is InjectionException ie && ie.DiagnosticData.Id != Common.Diagnostics.AggregateError.Id
                    ? null
                    : Common.Diagnostics.UnexpectedError),
                $"An error occurred while {description}.",
                location,
                e
            );
        }
    }
    
    public static T TryAggregate<T>(GeneratorExecutionContext context, Func<InjectionException, T> func) {
        var aggregateException = CreateAggregator(context);
        try {
            var result = func(aggregateException);
            aggregateException.ThrowIfAggregate();
            return result;
        } catch (Exception e) {
            ReportUnexpectedError(context, e);
            aggregateException.Diagnostics.AddRange(GetDiagnosticsFromException(e));
            throw aggregateException;
        }
    }
    
    public static void Try(Action action, string description, GeneratorExecutionContext context) => Try(action, description, Location.None, context);
    
    public static void Try(Action action, string description, Location location, GeneratorExecutionContext context) {
        try {
            action();
        } catch (Exception e) {
            throw new InjectionException(context, (e is InjectionException ie && ie.DiagnosticData.Id != Common.Diagnostics.AggregateError.Id
                    ? null
                    : Common.Diagnostics.UnexpectedError),
                $"An error occurred while {description}.",
                location,
                e
            );
        }
    }
    
    
    public static void TryAggregate(GeneratorExecutionContext context, Action<InjectionException> action) {
        var aggregateException = CreateAggregator(context);
        aggregateException.Aggregate(context, () => action(aggregateException));
        aggregateException.ThrowIfAggregate();
    }

    private static void ReportUnexpectedError(GeneratorExecutionContext context, Exception e) {
        if (e is not InjectionException) {
            context.ReportDiagnostic(Common.Diagnostics.UnexpectedError.CreateDiagnostic(e.ToString()));
        }
    }
    
    private static IList<Diagnostic> GetDiagnosticsFromException(Exception e) {
        return e is InjectionException ie
            ? ie.Diagnostics
            : ImmutableList.Create(Common.Diagnostics.UnexpectedError.CreateDiagnostic(e.ToString()));
    }

    private static InjectionException CreateAggregator(GeneratorExecutionContext context) {
        return new InjectionException(context, ImmutableList.Create<Exception>());
    }
}

public static class InjectionExceptionExtensions {
    public static IEnumerable<TResult> SelectCatching<TSource, TResult>(
        this IEnumerable<TSource> source,
        GeneratorExecutionContext context,
        Func<TSource, TResult> selector
    ) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        
        var results = new List<TResult>();
        var exceptions = new List<Exception>();
        foreach (var s in source) {
            try {
                results.Add(selector(s));
            } catch (Exception e) {
                exceptions.Add(e);
            }
        }

        if (exceptions.Any()) {
            throw new InjectionException(context, exceptions);
        }

        return results;
    }
}
