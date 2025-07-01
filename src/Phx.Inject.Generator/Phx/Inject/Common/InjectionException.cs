// -----------------------------------------------------------------------------
//  <copyright file="InjectionException.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common;

internal class InjectionException : Exception {
    public Diagnostics.DiagnosticData DiagnosticData { get; }
    public Location Location { get; }
    public List<Diagnostic> Diagnostics { get; } = new(); 

    public InjectionException(
        IEnumerable<Exception> aggregateExceptions
    ) : base(Common.Diagnostics.AggregateError.Title) {
        DiagnosticData = Common.Diagnostics.AggregateError;
        Location = Location.None;

        if (!aggregateExceptions.Any()) {
            Diagnostics.Add(Common.Diagnostics.CreateUnexpectedErrorDiagnostic("An unexpected error occurred, but no exceptions were provided."));
        }
        
        foreach (var e in aggregateExceptions) {
            if (e is InjectionException ie) {
                Diagnostics.AddRange(ie.Diagnostics);
            } else {
                Diagnostics.Add(Common.Diagnostics.CreateUnexpectedErrorDiagnostic(e?.ToString() ?? "[null]"));
            }
        }
    }
    
    public InjectionException(
        Diagnostics.DiagnosticData diagnosticData,
        string message,
        Location location
    ) : base(message) {
        DiagnosticData = diagnosticData;
        Location = location;
        Diagnostics.Add(Diagnostic.Create(
            new DiagnosticDescriptor(
                diagnosticData.Id,
                diagnosticData.Title,
                message,
                diagnosticData.Category,
                DiagnosticSeverity.Error,
                true),
            location));
    }

    public InjectionException(
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
        Diagnostics.Add(Diagnostic.Create(
            new DiagnosticDescriptor(
                DiagnosticData.Id,
                DiagnosticData.Title,
                message,
                DiagnosticData.Category,
                DiagnosticSeverity.Error,
                true),
            location));
        
        if (inner is InjectionException innerInjectionException) {
            Diagnostics.AddRange(innerInjectionException.Diagnostics);
        } else {
            Diagnostics.Add(Common.Diagnostics.CreateUnexpectedErrorDiagnostic(inner.ToString()));
        }
    }

    public static T Try<T>(Func<T> func, string description) => Try(func, description, Location.None);
    
    public static T Try<T>(Func<T> func, string description, Location location) {
        try {
            return func();
        } catch (Exception e) {
            throw new InjectionException((e is InjectionException ie && ie.DiagnosticData.Id != Common.Diagnostics.AggregateError.Id
                    ? null
                    : Common.Diagnostics.UnexpectedError),
                $"An error occurred while {description}.",
                location,
                e
            );
        }
    }
    
    public static void Try(Action action, string description) => Try(action, description, Location.None);
    
    public static void Try(Action action, string description, Location location) {
        try {
            action();
        } catch (Exception e) {
            throw new InjectionException((e is InjectionException ie && ie.DiagnosticData.Id != Common.Diagnostics.AggregateError.Id
                    ? null
                    : Common.Diagnostics.UnexpectedError),
                $"An error occurred while {description}.",
                location,
                e
            );
        }
    }
}

public static class InjectionExceptionExtensions {
    public static IEnumerable<TResult> SelectCatching<TSource, TResult>(
        this IEnumerable<TSource> source,
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
            throw new InjectionException(exceptions);
        }

        return results;
    }
}
