// -----------------------------------------------------------------------------
// <copyright file="DiagnosticsRecorder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Diagnostics;

/// <summary>
///     Concrete implementation of <see cref="IDiagnosticsRecorder"/> that accumulates diagnostics
///     in an in-memory list for batch processing.
/// </summary>
/// <remarks>
///     Uses simple <c>List&lt;DiagnosticInfo&gt;</c> (thread-local, minimal allocations). The
///     <c>Capture</c> method wraps user code and converts exceptions to Error results, ensuring
///     unexpected errors appear as diagnostic errors instead of crashing compilation.
/// </remarks>
internal sealed class DiagnosticsRecorder : IDiagnosticsRecorder {
    /// <summary>
    ///     Internal collection of accumulated diagnostics. Growable to handle any number of errors.
    /// </summary>
    private readonly List<DiagnosticInfo> diagnostics = new();
    
    /// <inheritdoc />
    public void Add(DiagnosticInfo diagnosticInfo) {
        diagnostics.Add(diagnosticInfo);
    }

    /// <inheritdoc />
    public void Add(IEnumerable<DiagnosticInfo> diagnosticInfos) {
        diagnostics.AddRange(diagnosticInfos);
    }
    
    /// <summary>
    ///     Executes a function within an exception-safe diagnostic capture context.
    /// </summary>
    /// <typeparam name="T">
    ///     The success result type. Must be equatable for incremental caching.
    /// </typeparam>
    /// <param name="func">
    ///     The function to execute. Receives a thread-local recorder to accumulate diagnostics.
    /// </param>
    /// <returns>
    ///     Ok result if function succeeds (may include warnings). Error result if function throws
    ///     <c>GeneratorException</c> or an unexpected exception.
    /// </returns>
    /// <remarks>
    ///     Converts <c>GeneratorException</c> (expected, with diagnostics) to Error results.
    ///     Converts other exceptions (unexpected bugs) to error diagnostics with <see cref="DiagnosticType.InternalError"/>,
    ///     ensuring real errors appear as user-visible compilation errors instead of hidden in Roslyn's exception handling.
    ///     Thread-safe: creates a new recorder per invocation, so concurrent calls each get isolated diagnostic collections.
    /// </remarks>
    public static IResult<T> Capture<T>(Func<IDiagnosticsRecorder, T> func) where T : IEquatable<T> {
        var recorder = new DiagnosticsRecorder();
        try {
            var result = func(recorder);
            return result.ToOkResult(recorder.diagnostics.ToEquatableList());
        } catch (GeneratorException ex) {
            recorder.Add(ex.DiagnosticInfos);
            return Result.Error<T>(recorder.diagnostics.ToEquatableList());
        } catch (Exception ex) {
            var errorMessage = $"Generator bug: {ex.GetType().Name}: {ex.Message}";
            recorder.Add(new DiagnosticInfo(DiagnosticType.InternalError, errorMessage, null));
            return Result.Error<T>(recorder.diagnostics.ToEquatableList());
        }
    }
}