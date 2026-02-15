// -----------------------------------------------------------------------------
// <copyright file="DiagnosticInfo.cs" company="Star Cruise Studios LLC">
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
///     Represents a diagnostic message to be reported during code generation.
/// </summary>
/// <param name="Type"> The type of diagnostic. </param>
/// <param name="Message"> The diagnostic message. </param>
/// <param name="Location"> The source code location where the diagnostic occurred. </param>
internal sealed record DiagnosticInfo(
    DiagnosticType Type,
    string Message,
    LocationInfo? Location
) {
    /// <summary>
    ///     Reports this diagnostic to the Roslyn source production context.
    /// </summary>
    /// <param name="context"> The source production context. </param>
    public void Report(SourceProductionContext context) {
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    Type.Id,
                    Type.Title,
                    Message,
                    Type.Category,
                    Type.Severity,
                    Type.IsEnabledByDefault
                ),
                Location?.ToLocation()
            )
        );
    }
}
