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
///     Result type that encapsulates either a success value or validation failures,
///     enabling Railway-Oriented Programming for error handling in the generator pipeline.
/// </summary>
/// <typeparam name="T">
///     The success value type. Must be equatable for incremental generator caching.
/// </typeparam>
/// <remarks>
///     <para>Design Philosophy:</para>
///     <para>
///     Replaces exception-based error handling with explicit result types. This ensures
///     ALL validation errors in a file are collected and reported, not just the first one.
///     Users get comprehensive error feedback in a single compilation pass.
///     </para>
///     
///     <para>Railway-Oriented Programming:</para>
///     <para>
///     Results flow through transformer pipelines on either a "success track" (carrying values)
///     or an "error track" (carrying diagnostics). The <c>Map</c> operation short-circuits
///     error results, automatically propagating errors without explicit null checks everywhere.
///     </para>
///     
///     <para>Incremental Caching:</para>
///     <para>
///     Results are cached by Roslyn's incremental system. Diagnostics are included in equality
///     comparisons, so changing an error message triggers recompilation. This is intentional -
///     users should see updated error messages even if the underlying issue hasn't changed.
///     </para>
///     
///     <para>Thread Safety:</para>
///     <para>
///     Result instances are immutable and thread-safe. Multiple threads can read the same
///     result concurrently. However, the <c>GetValue</c> operation mutates the provided
///     <c>IDiagnosticsRecorder</c>, so that recorder must be thread-local.
///     </para>
///     
///     <para>When to use Ok vs Error:</para>
///     <list type="bullet">
///         <item>Ok: Transformation succeeded, value is valid (may include warnings)</item>
///         <item>Error: Transformation failed, no valid value exists (always has errors)</item>
///     </list>
///     
///     <para>DO NOT use for:</para>
///     <para>
///     Internal generator bugs or infrastructure failures. Those should throw exceptions.
///     Results are ONLY for user code validation errors that should be presented to developers.
///     </para>
/// </remarks>
internal interface IResult<out T> where T : IEquatable<T>? {
    /// <summary>
    ///     Indicates whether this result represents a successful transformation.
    /// </summary>
    /// <remarks>
    ///     <c>true</c> means <c>GetValue</c> will return a valid value.
    ///     <c>false</c> means <c>GetValue</c> will throw and only diagnostics are available.
    ///     Always check this before calling <c>GetValue</c> unless you're using helper methods
    ///     like <c>OrNull</c> or <c>OrElse</c> that handle both cases.
    /// </remarks>
    bool IsOk { get; }
    
    /// <summary>
    ///     Gets all diagnostic messages (errors, warnings, info) associated with this result.
    /// </summary>
    /// <remarks>
    ///     Present in both Ok and Error results. Ok results may include warnings.
    ///     Error results always have at least one error-level diagnostic.
    ///     This collection participates in equality comparison for incremental caching.
    /// </remarks>
    EquatableList<DiagnosticInfo> DiagnosticInfo { get; }

    /// <summary>
    ///     Extracts the success value and records all diagnostics to the provided recorder.
    /// </summary>
    /// <param name="diagnostics">
    ///     Thread-local recorder to accumulate diagnostics. Must not be shared across threads.
    /// </param>
    /// <returns>The success value if <c>IsOk</c> is true.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called on an Error result. Check <c>IsOk</c> first or use safe helpers.
    /// </exception>
    /// <remarks>
    ///     Side effect: Adds this result's diagnostics to the recorder. This is the
    ///     primary mechanism for collecting all errors from a pipeline execution.
    /// </remarks>
    T GetValue(IDiagnosticsRecorder diagnostics);
    
    /// <summary>
    ///     Monadic bind operation for chaining transformations on success values.
    /// </summary>
    /// <typeparam name="R">The type of the transformed result value.</typeparam>
    /// <param name="mapFunc">
    ///     Transformation to apply if this is an Ok result. Not invoked on Error results.
    /// </param>
    /// <returns>
    ///     If this is Ok: applies <paramref name="mapFunc"/> to the value, returns its result.
    ///     If this is Error: returns a new Error result with the same diagnostics.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///     This enables Railway-Oriented Programming where errors automatically short-circuit
    ///     through a chain of transformations. You can chain multiple <c>Map</c> calls without
    ///     checking for errors at each step - errors propagate automatically.
    ///     </para>
    ///     
    ///     <example>
    ///     <code>
    ///     result
    ///         .Map(ParseAttributes)
    ///         .Map(ValidateTypes)
    ///         .Map(GenerateCode)
    ///     // If any step fails, subsequent steps are skipped
    ///     </code>
    ///     </example>
    /// </remarks>
    IResult<R> Map<R>(Func<T, IResult<R>> mapFunc) where R : IEquatable<R>?;
    
    /// <summary>
    ///     Converts an Error result to a different result type, preserving the diagnostics.
    /// </summary>
    /// <typeparam name="R">The new result value type.</typeparam>
    /// <returns>An Error result with type R containing the same diagnostics.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called on an Ok result. This operation is only valid for Error results.
    /// </exception>
    /// <remarks>
    ///     Used when an error needs to propagate up through differently-typed pipeline stages.
    ///     Allows error information to flow through the type system without loss.
    /// </remarks>
    IResult<R> MapError<R>() where R : IEquatable<R>?;
}

/// <summary>
///     Represents a successful result with a value.
/// </summary>
/// <typeparam name="T"> The type of the result value. </typeparam>
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
/// <typeparam name="T"> The type of the result value. </typeparam>
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
///     Factory methods and extension methods for working with <see cref="IResult{T}"/> instances.
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