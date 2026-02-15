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
///     <para>Design Decision:</para>
///     <para>
///     Uses a simple <c>List&lt;DiagnosticInfo&gt;</c> rather than a concurrent collection
///     because each recorder is thread-local. Allocations are minimal since diagnostics are
///     typically infrequent (clean code produces few errors).
///     </para>
///     
///     <para>Exception Handling Strategy:</para>
///     <para>
///     The <c>Capture</c> method wraps user code and converts exceptions into Error results.
///     This ensures that even when transformers throw (which they shouldn't, but might due to
///     bugs), the error is captured as a diagnostic rather than crashing the entire compilation.
///     This provides graceful degradation - other files still compile even if one has issues.
///     </para>
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
    ///     <list type="bullet">
    ///         <item>Ok result: Function completed successfully, may include warnings</item>
    ///         <item>Error result: Function threw <c>GeneratorException</c> or an unexpected exception</item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     <para>Exception Handling Strategy:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <term>GeneratorException:</term>
    ///             <description>
    ///             Expected exception carrying validation diagnostics. Captured and returned
    ///             as Error result with those diagnostics.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Other exceptions:</term>
    ///             <description>
    ///             Unexpected generator bugs. Caught and converted to error diagnostics with
    ///             <see cref="DiagnosticType.InternalError"/>. This ensures the actual error
    ///             appears as a compilation error visible to the user instead of being hidden
    ///             in Roslyn's generic exception handling. The exception message is preserved.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>Why Catch All Exceptions?</para>
    ///     <para>
    ///     If we let unexpected exceptions propagate to Roslyn's top-level exception handler,
    ///     they get converted to opaque compiler errors that hide the actual problem. By catching
    ///     them here and converting to diagnostics, the real error (null reference, index out of bounds,
    ///     etc.) appears clearly as a compilation error the user can investigate.
    ///     </para>
    ///     
    ///     <para>Usage Pattern:</para>
    ///     <para>
    ///     Wrap complex transformation logic in <c>Capture</c> to ensure all exceptions,
    ///     expected and unexpected, are converted to user-visible compilation errors.
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
    ///     <para>Thread Safety:</para>
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
        } catch (Exception ex) {
            var errorMessage = $"Generator bug: {ex.GetType().Name}: {ex.Message}";
            recorder.Add(new DiagnosticInfo(DiagnosticType.InternalError, errorMessage, null));
            return Result.Error<T>(recorder.diagnostics.ToEquatableList());
        }
    }
}