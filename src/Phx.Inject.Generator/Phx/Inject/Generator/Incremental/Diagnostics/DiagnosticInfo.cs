// -----------------------------------------------------------------------------
// <copyright file="DiagnosticInfo.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Diagnostics;

internal record DiagnosticInfo(
    DiagnosticType Type,
    string Message,
    LocationInfo? Location
) {
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
