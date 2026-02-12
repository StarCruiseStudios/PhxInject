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
///     <para><b>Design Decision:</b></para>
///     <para>
///     Uses a simple <c>List&lt;DiagnosticInfo&gt;</c> rather than a concurrent collection
///     because each recorder is thread-local. Allocations are minimal since diagnostics are
///     typically infrequent (clean code produces few errors).
///     </para>
///     
///     <para><b>Exception Handling Strategy:</b></para>
///     <para>
///     The <c>Capture</c> method wraps user code and converts exceptions into Error results.
///     This ensures that even when transformers throw (which they shouldn't, but might due to
///     bugs), the error is captured as a diagnostic rather than crashing the entire compilation.
///     This provides graceful degradation - other files still compile even if one has issues.
///     </para>
/// </remarks>
internal class DiagnosticsRecorder : IDiagnosticsRecorder {
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
    ///     <list type="bullet">
    ///         <item>Ok result: Function completed successfully, may include warnings</item>
    ///         <item>Error result: Function threw <c>GeneratorException</c> or returned error diagnostics</item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     <para><b>Exception Handling:</b></para>
    ///     <list type="bullet">
    ///         <item>
    ///             <b>GeneratorException:</b> Expected exception carrying diagnostics.
    ///             Captured and returned as Error result with those diagnostics.
    ///         </item>
    ///         <item>
    ///             <b>Other exceptions:</b> Unexpected (generator bugs). Caught and returned
    ///             as Error result, preventing compilation crash. However, this indicates
    ///             a generator implementation error that should be fixed.
    ///         </item>
    ///     </list>
    ///     
    ///     <para><b>Usage Pattern:</b></para>
    ///     <para>
    ///     Wrap complex transformation logic in <c>Capture</c> to ensure exceptions don't
    ///     escape and crash the compiler. The recorder is local to this execution and
    ///     automatically aggregates all diagnostics before returning.
    ///     </para>
    ///     
    ///     <example>
    ///     <code>
    ///     var result = DiagnosticsRecorder.Capture(diagnostics => {
    ///         var metadata = ParseSyntax(node);
    ///         var validated = ValidateMetadata(metadata, diagnostics);
    ///         return validated;
    ///     });
    ///     </code>
    ///     </example>
    ///     
    ///     <para><b>Thread Safety:</b></para>
    ///     <para>
    ///     Creates a new recorder per invocation, so multiple threads can safely call
    ///     <c>Capture</c> concurrently. Each gets its own isolated diagnostic collection.
    ///     </para>
    /// </remarks>
    public static IResult<T> Capture<T>(Func<IDiagnosticsRecorder, T> func) where T : IEquatable<T> {
        var recorder = new DiagnosticsRecorder();
        try {
            var result = func(recorder);
            return result.ToOkResult(recorder.diagnostics.ToEquatableList());
        } catch (GeneratorException ex) {
            recorder.Add(ex.DiagnosticInfos);
            return Result.Error<T>(recorder.diagnostics.ToEquatableList());
        } catch {
            return Result.Error<T>(recorder.diagnostics.ToEquatableList());
        }
    }
}