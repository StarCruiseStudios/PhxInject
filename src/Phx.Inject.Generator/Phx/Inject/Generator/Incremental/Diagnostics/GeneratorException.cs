// -----------------------------------------------------------------------------
// <copyright file="GeneratorException.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Diagnostics;

internal class GeneratorException : Exception {
    public EquatableList<DiagnosticInfo> DiagnosticInfos { get; }

    public GeneratorException(EquatableList<DiagnosticInfo> diagnosticInfos)
        : base(BuildMessage(diagnosticInfos)) {
        DiagnosticInfos = diagnosticInfos;
    }

    public GeneratorException(EquatableList<DiagnosticInfo> diagnosticInfos, Exception? innerException)
        : base(BuildMessage(diagnosticInfos), innerException) {
        DiagnosticInfos = diagnosticInfos;
    }

    public GeneratorException(params DiagnosticInfo[] diagnosticInfos)
        : this(diagnosticInfos.ToEquatableList()) {
    }

    public GeneratorException(Exception? innerException, params DiagnosticInfo[] diagnosticInfos)
        : this(diagnosticInfos.ToEquatableList(), innerException) {
    }

    private static string BuildMessage(EquatableList<DiagnosticInfo> diagnosticInfos) {
        return diagnosticInfos.Count switch {
            0 => "A fatal generator error occurred.",
            1 => diagnosticInfos[0].Message,
            _ => $"Multiple fatal generator errors occurred ({diagnosticInfos.Count} total)."
        };
    }
}
