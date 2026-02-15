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

/// <summary>
///     Exception thrown during code generation that contains diagnostic information.
/// </summary>
internal sealed class GeneratorException : Exception {
    /// <summary> The diagnostics associated with this exception. </summary>
    public EquatableList<DiagnosticInfo> DiagnosticInfos { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GeneratorException"/> class.
    /// </summary>
    /// <param name="diagnosticInfos"> The diagnostics associated with this exception. </param>
    public GeneratorException(EquatableList<DiagnosticInfo> diagnosticInfos)
        : base(BuildMessage(diagnosticInfos)) {
        DiagnosticInfos = diagnosticInfos;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GeneratorException"/> class with an inner exception.
    /// </summary>
    /// <param name="diagnosticInfos"> The diagnostics associated with this exception. </param>
    /// <param name="innerException"> The inner exception. </param>
    public GeneratorException(EquatableList<DiagnosticInfo> diagnosticInfos, Exception? innerException)
        : base(BuildMessage(diagnosticInfos), innerException) {
        DiagnosticInfos = diagnosticInfos;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GeneratorException"/> class.
    /// </summary>
    /// <param name="diagnosticInfos"> The diagnostics associated with this exception. </param>
    public GeneratorException(params DiagnosticInfo[] diagnosticInfos)
        : this(diagnosticInfos.ToEquatableList()) {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GeneratorException"/> class with an inner exception.
    /// </summary>
    /// <param name="innerException"> The inner exception. </param>
    /// <param name="diagnosticInfos"> The diagnostics associated with this exception. </param>
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
