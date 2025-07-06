// -----------------------------------------------------------------------------
// <copyright file="Optional.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator;

namespace Phx.Inject.Common.Exceptions;

internal interface IResult<out T> {
    bool IsOk { get; }

    T GetValue();
    T GetOrThrow(IGeneratorContext generatorContext);
    T GetOrThrowFatal(IGeneratorContext generatorContext);
    IResult<R> Map<R>(Func<T, IResult<R>> mapFunc);
    IResult<R> MapError<R>();
}

internal static class Result {
    public static IResult<T> Ok<T>(T value) {
        return new OkResult<T>(value);
    }

    public static IResult<T> Error<T>(
        string message,
        Location location,
        Diagnostics.DiagnosticData diagnosticData
    ) {
        return new ErrorResult<T>(message, location, diagnosticData);
    }

    public static IResult<T> ErrorIfNull<T>(
        T? value,
        string message,
        Location location,
        Diagnostics.DiagnosticData diagnosticData
    ) {
        return value == null
            ? new ErrorResult<T>(message, location, diagnosticData)
            : new OkResult<T>(value);
    }
    
    public static IResult<R?> MapNullable<T, R>(
        this IResult<T?> result,
        Func<T, IResult<R>> mapFunc
    ) where R : class? {
        return result.Map(value => value == null
            ? Ok<R?>(null)
            : mapFunc(value));
    }
    
    private class OkResult<T> : IResult<T> {
        private readonly T value;

        public OkResult(T value) {
            this.value = value;
        }

        public bool IsOk {
            get => true;
        }

        public T GetValue() {
            return value;
        }

        public T GetOrThrow(IGeneratorContext generatorContext) {
            return value;
        }
        public T GetOrThrowFatal(IGeneratorContext generatorContext) {
            return value;
        }

        public IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) {
            return mapFunc(value);
        }
        public IResult<R> MapError<R>() {
            throw new InvalidOperationException("Cannot map error from an Ok result.");
        }
    }

    private class ErrorResult<T> : IResult<T> {
        private readonly Diagnostics.DiagnosticData diagnosticData;
        private readonly Location location;
        private readonly string message;

        public ErrorResult(string message, Location location, Diagnostics.DiagnosticData diagnosticData) {
            this.message = message;
            this.location = location;
            this.diagnosticData = diagnosticData;
        }

        public bool IsOk {
            get => false;
        }

        public T GetValue() {
            throw new InvalidOperationException("Cannot get value from an Error result.");
        }

        public T GetOrThrow(IGeneratorContext generatorContext) {
            throw diagnosticData.AsException(message, location, generatorContext);
        }

        public T GetOrThrowFatal(IGeneratorContext generatorContext) {
            throw diagnosticData.AsFatalException(message, location, generatorContext);
        }

        public IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) {
            return new ErrorResult<R>(message, location, diagnosticData);
        }
        public IResult<R> MapError<R>() {
            return new ErrorResult<R>(message, location, diagnosticData);
        }
    }
}
