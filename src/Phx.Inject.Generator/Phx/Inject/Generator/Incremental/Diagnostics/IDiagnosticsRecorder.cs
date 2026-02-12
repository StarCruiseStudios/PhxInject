// -----------------------------------------------------------------------------
// <copyright file="IDiagnosticsRecorder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Incremental.Diagnostics;

/// <summary>
///     Thread-local accumulator for collecting validation errors and warnings during pipeline execution.
/// </summary>
/// <remarks>
///     <para><b>Purpose:</b></para>
///     <para>
///     Provides a mutable collection point for diagnostics as transformers process syntax trees.
///     Enables "collect all errors, report all errors" semantics rather than failing fast on
///     the first problem. Users see comprehensive feedback in a single compilation.
///     </para>
///     
///     <para><b>Thread Safety:</b></para>
///     <para>
///     <b>NOT thread-safe.</b> Each pipeline execution must use its own recorder instance.
///     Roslyn may process multiple files concurrently, so never share recorders across threads.
///     The <c>Capture</c> method creates a thread-local recorder automatically.
///     </para>
///     
///     <para><b>Usage Pattern:</b></para>
///     <list type="number">
///         <item>Create a recorder (typically via <c>DiagnosticsRecorder.Capture</c>)</item>
///         <item>Pass it to transformers as they extract <c>IResult</c> values</item>
///         <item>Diagnostics accumulate as results are unwrapped with <c>GetValue</c></item>
///         <item>At the end, recorder contains all errors/warnings from that execution</item>
///     </list>
///     
///     <para><b>Architectural Role:</b></para>
///     <para>
///     Decouples error generation (in transformers) from error reporting (in generator output).
///     Transformers produce <c>IResult</c> objects with diagnostics; recorders aggregate them;
///     the generator's output stage reports them to Roslyn's diagnostic system.
///     </para>
/// </remarks>
internal interface IDiagnosticsRecorder {
    /// <summary>
    ///     Appends a single diagnostic to the accumulated collection.
    /// </summary>
    /// <param name="diagnosticInfo">
    ///     The diagnostic to record. Can be error, warning, or info severity.
    /// </param>
    /// <remarks>
    ///     Diagnostics are ordered by insertion. This preserves the logical flow of errors
    ///     as code is processed top-to-bottom, left-to-right.
    /// </remarks>
    void Add(DiagnosticInfo diagnosticInfo);
    
    /// <summary>
    ///     Appends multiple diagnostics to the accumulated collection.
    /// </summary>
    /// <param name="diagnosticInfos">
    ///     The diagnostics to record. Order is preserved.
    /// </param>
    /// <remarks>
    ///     Typically called when extracting values from <c>IResult</c> objects, which carry
    ///     their own diagnostic collections. This bulk operation is more efficient than
    ///     adding diagnostics one at a time.
    /// </remarks>
    void Add(IEnumerable<DiagnosticInfo> diagnosticInfos);
}