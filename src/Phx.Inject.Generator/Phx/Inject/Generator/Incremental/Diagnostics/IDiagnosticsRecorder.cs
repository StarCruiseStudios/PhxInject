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
///     NOT thread-safe. Each pipeline execution must use its own recorder instance. Transforms
///     produce <c>IResult</c> objects with diagnostics; recorders aggregate them; generator output
///     reports them to Roslyn. Enables "collect all errors, report all errors" in a single compilation.
/// </remarks>
internal interface IDiagnosticsRecorder {
    /// <summary>
    ///     Appends a single diagnostic to the accumulated collection.
    /// </summary>
    /// <param name="diagnosticInfo">The diagnostic to record (error, warning, or info).</param>
    /// <remarks>Preserves insertion order for logical error flow.</remarks>
    void Add(DiagnosticInfo diagnosticInfo);
    
    /// <summary>
    ///     Appends multiple diagnostics to the accumulated collection.
    /// </summary>
    /// <param name="diagnosticInfos">The diagnostics to record (order preserved).</param>
    /// <remarks>
    ///     Called when extracting values from <c>IResult</c> objects. More efficient than adding one at a time.
    /// </remarks>
    void Add(IEnumerable<DiagnosticInfo> diagnosticInfos);
}