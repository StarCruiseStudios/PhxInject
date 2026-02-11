// -----------------------------------------------------------------------------
// <copyright file="Result.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Diagnostics;

internal interface IResult<out T> where T : IEquatable<T>? {
    bool IsOk { get; }
    EquatableList<DiagnosticInfo> DiagnosticInfo { get; }

    T GetValue();
    IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) where R : IEquatable<R>?;
    IResult<R> MapError<R>() where R : IEquatable<R>?;
}

internal sealed class OkResult<T> : IResult<T> where T : IEquatable<T>? {
    private readonly T value;
    private readonly EquatableList<DiagnosticInfo> diagnosticInfo;

    public OkResult(T value, EquatableList<DiagnosticInfo> diagnosticInfo) {
        this.value = value;
        this.diagnosticInfo = diagnosticInfo;
    }

    public EquatableList<DiagnosticInfo> DiagnosticInfo => diagnosticInfo;

    public bool IsOk => true;

    public T GetValue() {
        return value;
    }

    public IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) where R : IEquatable<R>? {
        return mapFunc(value);
    }

    public IResult<R> MapError<R>() where R : IEquatable<R>? {
        throw new InvalidOperationException("Cannot map error from an Ok result.");
    }
}

internal sealed class ErrorResult<T> : IResult<T> where T : IEquatable<T>? {
    private readonly EquatableList<DiagnosticInfo> diagnosticInfo;

    public ErrorResult(EquatableList<DiagnosticInfo> diagnosticInfo) {
        this.diagnosticInfo = diagnosticInfo;
    }

    public EquatableList<DiagnosticInfo> DiagnosticInfo => diagnosticInfo;

    public bool IsOk => false;

    public T GetValue() {
        throw new InvalidOperationException("Cannot get value from an Error result.");
    }

    public IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) where R : IEquatable<R>? {
        return Result.Error<R>(DiagnosticInfo);
    }

    public IResult<R> MapError<R>() where R : IEquatable<R>? {
        return Result.Error<R>(DiagnosticInfo);
    }
}

internal static class Result {
    public static IResult<T> Ok<T>(T value, params DiagnosticInfo[] diagnosticInfo) where T : IEquatable<T>? {
        return new OkResult<T>(value, diagnosticInfo.ToEquatableList());
    }

    public static IResult<T> Ok<T>(T value, EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return new OkResult<T>(value, diagnosticInfo);
    }

    public static IResult<T> Error<T>(params DiagnosticInfo[] diagnosticInfo) where T : IEquatable<T>? {
        return new ErrorResult<T>(diagnosticInfo.ToEquatableList());
    }

    public static IResult<T> Error<T>(EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return new ErrorResult<T>(diagnosticInfo);
    }
    
    public static IResult<T> ToOkResult<T>(this T value, params DiagnosticInfo[] diagnosticInfo) where T : IEquatable<T>? {
        return Result.Ok(value, diagnosticInfo);
    }

    public static IResult<T> ToOkResult<T>(this T value, EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return Result.Ok(value, diagnosticInfo);
    }

    public static IResult<T> ToErrorResult<T>(this EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return Result.Error<T>(diagnosticInfo);
    }

    public static IResult<T> ToErrorResult<T>(this DiagnosticInfo diagnosticInfo) where T : IEquatable<T>? {
        return Result.Error<T>(EquatableList<DiagnosticInfo>.Create(diagnosticInfo));
    }

    public static IncrementalValueProvider<EquatableList<DiagnosticInfo>> SelectDiagnostics<T>(
        this IncrementalValueProvider<IResult<T>> provider
    ) where T : IEquatable<T>? {
        return provider.Select((result, _) => result.DiagnosticInfo);
    }

    public static IncrementalValueProvider<EquatableList<DiagnosticInfo>> SelectDiagnostics<T>(
        this IncrementalValuesProvider<IResult<T>> provider
    ) where T : IEquatable<T>? {
        return provider
            .SelectMany((result, _) => result.DiagnosticInfo)
            .Collect()
            .Select((diagnostics, _) => diagnostics.ToEquatableList());
    }
}