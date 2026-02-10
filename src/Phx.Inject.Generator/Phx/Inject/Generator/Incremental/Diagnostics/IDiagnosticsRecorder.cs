// -----------------------------------------------------------------------------
// <copyright file="IDiagnosticsRecorder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Incremental.Diagnostics;

internal interface IDiagnosticsRecorder {
    void Add(DiagnosticInfo diagnosticInfo);
    void Add(IEnumerable<DiagnosticInfo> diagnosticInfos);
}