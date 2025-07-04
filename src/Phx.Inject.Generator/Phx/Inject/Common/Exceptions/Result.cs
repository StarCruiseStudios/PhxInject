// -----------------------------------------------------------------------------
// <copyright file="Optional.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common.Exceptions;

public interface IResult<out T> {
    bool IsOk { get; }

    T GetValue();
    T GetOrThrow(GeneratorExecutionContext generatorExecutionContext);
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

    public static IResult<T> FromNullable<T>(
        T? value,
        string message,
        Location location,
        Diagnostics.DiagnosticData diagnosticData
    ) {
        return value == null
            ? new ErrorResult<T>(message, location, diagnosticData)
            : new OkResult<T>(value);
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

        public T GetOrThrow(GeneratorExecutionContext generatorExecutionContext) {
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

        public T GetOrThrow(GeneratorExecutionContext generatorExecutionContext) {
            throw diagnosticData.AsException(message, location, generatorExecutionContext);
        }

        public IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) {
            return new ErrorResult<R>(message, location, diagnosticData);
        }
        public IResult<R> MapError<R>() {
            return new ErrorResult<R>(message, location, diagnosticData);
        }
    }
}
