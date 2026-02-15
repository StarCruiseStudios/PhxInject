// -----------------------------------------------------------------------------
// <copyright file="Result.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Diagnostics;

/// <summary>
///     Result type encapsulating either a success value or validation failures.
///     Enables Railway-Oriented Programming: transformations chain and errors short-circuit automatically.
/// </summary>
/// <typeparam name="T">
///     The success value type. Must be equatable for incremental generator caching.
/// </typeparam>
/// <remarks>
///     Collects ALL validation errors from a file instead of failing fast. Use for user code
///     validation errors only—internal generator bugs should throw exceptions. Results include
///     diagnostics in equality comparison, so error message changes trigger recompilation. Thread-safe
///     for reading; recorder mutations must be thread-local.
/// </remarks>
internal interface IResult<out T> where T : IEquatable<T>? {
    /// <summary>Indicates whether this result represents success.</summary>
    /// <remarks>If <c>true</c>, <c>GetValue</c> can be called safely. If <c>false</c>, diagnostics are available instead.</remarks>
    bool IsOk { get; }
    
    /// <summary>Gets all diagnostics (errors, warnings, info) associated with this result.</summary>
    /// <remarks>Present in both Ok and Error results. Participates in equality for incremental caching.</remarks>
    EquatableList<DiagnosticInfo> DiagnosticInfo { get; }

    /// <summary>
    ///     Extracts the success value and records all diagnostics to the provided recorder.
    /// </summary>
    /// <param name="diagnostics">Thread-local recorder (must not be shared across threads).</param>
    /// <returns>The success value if <c>IsOk</c> is true.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called on an Error result. Check <c>IsOk</c> first or use <c>OrNull</c>/<c>OrElse</c>.
    /// </exception>
    T GetValue(IDiagnosticsRecorder diagnostics);
    
    /// <summary>
    ///     Monadic bind operation for chaining transformations on success values.
    /// </summary>
    /// <typeparam name="R">The type of the transformed result value.</typeparam>
    /// <param name="mapFunc">
    ///     Transformation to apply if Ok. Not invoked if Error; returns the same diagnostics instead.
    /// </param>
    /// <returns>
    ///     Ok: applied result. Error: new Error result with same diagnostics.
    /// </returns>
    /// <remarks>
    ///     Enables Railway-Oriented Programming: errors automatically short-circuit through chains
    ///     without explicit error checks at each step.
    /// </remarks>
    IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) where R : IEquatable<R>?;
    
    /// <summary>
    ///     Converts an Error result to a different result type, preserving the diagnostics.
    /// </summary>
    /// <typeparam name="R">The new result value type.</typeparam>
    /// <returns>An Error result with type R containing the same diagnostics.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called on an Ok result. Only valid for Error results.
    /// </exception>
    IResult<R> MapError<R>() where R : IEquatable<R>?;
}

/// <summary>
///     Represents a successful result with a value.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
internal sealed class OkResult<T> : IResult<T> where T : IEquatable<T>? {
    /// <summary> The result value. </summary>
    private readonly T value;
    
    /// <summary> The diagnostics associated with this result. </summary>
    private readonly EquatableList<DiagnosticInfo> diagnosticInfo;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OkResult{T}"/> class.
    /// </summary>
    /// <param name="value"> The result value. </param>
    /// <param name="diagnosticInfo"> The diagnostics associated with this result. </param>
    public OkResult(T value, EquatableList<DiagnosticInfo> diagnosticInfo) {
        this.value = value;
        this.diagnosticInfo = diagnosticInfo;
    }

    /// <inheritdoc />
    public EquatableList<DiagnosticInfo> DiagnosticInfo => diagnosticInfo;

    /// <inheritdoc />
    public bool IsOk => true;

    /// <inheritdoc />
    public T GetValue(IDiagnosticsRecorder diagnostics) {
        diagnostics.Add(diagnosticInfo);
        return value;
    }

    /// <inheritdoc />
    public IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) where R : IEquatable<R>? {
        return mapFunc(value);
    }

    /// <inheritdoc />
    public IResult<R> MapError<R>() where R : IEquatable<R>? {
        throw new InvalidOperationException("Cannot map error from an Ok result.");
    }
}

/// <summary>
///     Represents a failed result with diagnostics.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
internal sealed class ErrorResult<T> : IResult<T> where T : IEquatable<T>? {
    /// <summary> The diagnostics associated with this error. </summary>
    private readonly EquatableList<DiagnosticInfo> diagnosticInfo;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorResult{T}"/> class.
    /// </summary>
    /// <param name="diagnosticInfo"> The diagnostics associated with this error. </param>
    public ErrorResult(EquatableList<DiagnosticInfo> diagnosticInfo) {
        this.diagnosticInfo = diagnosticInfo;
    }

    /// <inheritdoc />
    public EquatableList<DiagnosticInfo> DiagnosticInfo => diagnosticInfo;

    /// <inheritdoc />
    public bool IsOk => false;

    /// <inheritdoc />
    public T GetValue(IDiagnosticsRecorder diagnostics) {
        throw new InvalidOperationException("Cannot get value from an Error result.");
    }

    /// <inheritdoc />
    public IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) where R : IEquatable<R>? {
        return Result.Error<R>(DiagnosticInfo);
    }

    /// <inheritdoc />
    public IResult<R> MapError<R>() where R : IEquatable<R>? {
        return Result.Error<R>(DiagnosticInfo);
    }
}

/// <summary>
///     Factory and extension methods for <see cref="IResult{T}"/> instances.
/// </summary>
internal static class Result {
    /// <summary>
    ///     Creates a successful result with a value and diagnostics.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="value"> The result value. </param>
    /// <param name="diagnosticInfo"> The diagnostics associated with the result. </param>
    /// <returns> A successful result. </returns>
    public static IResult<T> Ok<T>(T value, params DiagnosticInfo[] diagnosticInfo) where T : IEquatable<T>? {
        return new OkResult<T>(value, diagnosticInfo.ToEquatableList());
    }

    /// <summary>
    ///     Creates a successful result with a value and diagnostics.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="value"> The result value. </param>
    /// <param name="diagnosticInfo"> The diagnostics associated with the result. </param>
    /// <returns> A successful result. </returns>
    public static IResult<T> Ok<T>(T value, EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return new OkResult<T>(value, diagnosticInfo);
    }

    /// <summary>
    ///     Creates an error result with diagnostics.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="diagnosticInfo"> The diagnostics associated with the error. </param>
    /// <returns> An error result. </returns>
    public static IResult<T> Error<T>(params DiagnosticInfo[] diagnosticInfo) where T : IEquatable<T>? {
        return new ErrorResult<T>(diagnosticInfo.ToEquatableList());
    }

    /// <summary>
    ///     Creates an error result with diagnostics.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="diagnosticInfo"> The diagnostics associated with the error. </param>
    /// <returns> An error result. </returns>
    public static IResult<T> Error<T>(EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return new ErrorResult<T>(diagnosticInfo);
    }
    
    /// <summary>
    ///     Converts a value to a successful result with diagnostics.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="value"> The result value. </param>
    /// <param name="diagnosticInfo"> The diagnostics associated with the result. </param>
    /// <returns> A successful result. </returns>
    public static IResult<T> ToOkResult<T>(this T value, params DiagnosticInfo[] diagnosticInfo) where T : IEquatable<T>? {
        return Ok(value, diagnosticInfo);
    }

    /// <summary>
    ///     Converts a value to a successful result with diagnostics.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="value"> The result value. </param>
    /// <param name="diagnosticInfo"> The diagnostics associated with the result. </param>
    /// <returns> A successful result. </returns>
    public static IResult<T> ToOkResult<T>(this T value, EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return Ok(value, diagnosticInfo);
    }

    /// <summary>
    ///     Converts diagnostics to an error result.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="diagnosticInfo"> The diagnostics associated with the error. </param>
    /// <returns> An error result. </returns>
    public static IResult<T> ToErrorResult<T>(this EquatableList<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return Error<T>(diagnosticInfo);
    }

    /// <summary>
    ///     Converts a diagnostic to an error result.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="diagnosticInfo"> The diagnostic associated with the error. </param>
    /// <returns> An error result. </returns>
    public static IResult<T> ToErrorResult<T>(this DiagnosticInfo diagnosticInfo) where T : IEquatable<T>? {
        return Result.Error<T>(EquatableList<DiagnosticInfo>.Create(diagnosticInfo));
    }
    
    /// <summary>
    ///     Tries to get the result value if successful, otherwise returns false.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="result"> The result to unwrap. </param>
    /// <param name="diagnostics"> The diagnostics recorder. </param>
    /// <param name="value"> The result value if successful. </param>
    /// <returns> True if successful, false otherwise. </returns>
    public static bool TryGetValue<T>(this IResult<T> result, IDiagnosticsRecorder diagnostics, out T value) where T : IEquatable<T>? {
        if (result.IsOk) {
            value = result.GetValue(diagnostics);
            return true;
        }

        value = default!;
        return false;
    }
    
    /// <summary>
    ///     Gets the result value if successful, otherwise returns null.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="result"> The result to unwrap. </param>
    /// <param name="diagnostics"> The diagnostics recorder. </param>
    /// <returns> The result value if successful, null otherwise. </returns>
    public static T? OrNull<T>(this IResult<T> result, IDiagnosticsRecorder diagnostics) where T : class, IEquatable<T>? {
        return result.IsOk ? result.GetValue(diagnostics) : null;
    }
    
    /// <summary>
    ///     Gets the result value if successful, otherwise returns a default value.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="result"> The result to unwrap. </param>
    /// <param name="diagnostics"> The diagnostics recorder. </param>
    /// <param name="defaultValue"> The function that provides the default value. </param>
    /// <returns> The result value if successful, the default value otherwise. </returns>
    public static T OrElse<T>(this IResult<T> result, IDiagnosticsRecorder diagnostics, Func<T> defaultValue) where T : IEquatable<T> {
        return result.IsOk ? result.GetValue(diagnostics) : defaultValue();
    }
    
    /// <summary>
    ///     Gets the result value if successful, otherwise throws a <see cref="GeneratorException"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="result"> The result to unwrap. </param>
    /// <param name="diagnostics"> The diagnostics recorder. </param>
    /// <returns> The result value. </returns>
    /// <exception cref="GeneratorException"> Thrown if the result is an error. </exception>
    public static T OrThrow<T>(this IResult<T> result, IDiagnosticsRecorder diagnostics) where T : IEquatable<T>? {
        return result.IsOk 
            ? result.GetValue(diagnostics)
            : throw new GeneratorException(result.DiagnosticInfo);
    }
    
    /// <summary>
    ///     Selects result values from a sequence, throwing exceptions for any errors.
    /// </summary>
    /// <typeparam name="T"> The type of the result values. </typeparam>
    /// <param name="results"> The sequence of results. </param>
    /// <param name="diagnostics"> The diagnostics recorder. </param>
    /// <returns> A sequence of unwrapped values. </returns>
    /// <exception cref="GeneratorException"> Thrown if any result is an error. </exception>
    public static IEnumerable<T> SelectOrThrow<T>(this IEnumerable<IResult<T>> results, IDiagnosticsRecorder diagnostics) where T : IEquatable<T>? {
        foreach (var result in results) {
            yield return result.OrThrow(diagnostics);
        }
    }

    /// <summary>
    ///     Selects diagnostics from an incremental value provider result.
    /// </summary>
    /// <typeparam name="T"> The type of the result value. </typeparam>
    /// <param name="provider"> The incremental value provider. </param>
    /// <returns> An incremental value provider of diagnostics. </returns>
    public static IncrementalValueProvider<EquatableList<DiagnosticInfo>> SelectDiagnostics<T>(
        this IncrementalValueProvider<IResult<T>> provider
    ) where T : IEquatable<T>? {
        return provider.Select((result, _) => result.DiagnosticInfo);
    }

    /// <summary>
    ///     Selects and collects diagnostics from an incremental values provider result.
    /// </summary>
    /// <typeparam name="T"> The type of the result values. </typeparam>
    /// <param name="provider"> The incremental values provider. </param>
    /// <returns> An incremental value provider of collected diagnostics. </returns>
    public static IncrementalValueProvider<EquatableList<DiagnosticInfo>> SelectDiagnostics<T>(
        this IncrementalValuesProvider<IResult<T>> provider
    ) where T : IEquatable<T>? {
        return provider
            .SelectMany((result, _) => result.DiagnosticInfo)
            .Collect()
            .Select((diagnostics, _) => diagnostics.ToEquatableList());
    }
}