// -----------------------------------------------------------------------------
// <copyright file="IDiagnosticsRecorder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Incremental.Diagnostics;

/// <summary>
///     Interface for recording diagnostic information during code generation.
/// </summary>
internal interface IDiagnosticsRecorder {
    /// <summary>
    ///     Adds a single diagnostic.
    /// </summary>
    /// <param name="diagnosticInfo"> The diagnostic to add. </param>
    void Add(DiagnosticInfo diagnosticInfo);
    
    /// <summary>
    ///     Adds multiple diagnostics.
    /// </summary>
    /// <param name="diagnosticInfos"> The diagnostics to add. </param>
    void Add(IEnumerable<DiagnosticInfo> diagnosticInfos);
}