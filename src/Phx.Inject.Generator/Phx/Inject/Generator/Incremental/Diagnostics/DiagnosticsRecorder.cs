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
///     Records diagnostic information during code generation for later reporting.
/// </summary>
internal class DiagnosticsRecorder : IDiagnosticsRecorder {
    /// <summary> The list of recorded diagnostics. </summary>
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
    ///     Executes a function while capturing any diagnostics it produces.
    /// </summary>
    /// <typeparam name="T"> The type of result value. </typeparam>
    /// <param name="func"> The function to execute. </param>
    /// <returns> A result containing the value and any captured diagnostics, or an error result if an exception occurred. </returns>
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