// -----------------------------------------------------------------------------
// <copyright file="DiagnosticsRecorder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Diagnostics;

internal class DiagnosticsRecorder : IDiagnosticsRecorder {
    private readonly List<DiagnosticInfo> diagnostics = new();
    
    public void Add(DiagnosticInfo diagnosticInfo) {
        diagnostics.Add(diagnosticInfo);
    }

    public void Add(IEnumerable<DiagnosticInfo> diagnosticInfos) {
        diagnostics.AddRange(diagnosticInfos);
    }
    
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