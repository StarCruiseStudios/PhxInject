// -----------------------------------------------------------------------------
// <copyright file="Result.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Diagnostics;

internal sealed record Result<T>(T Value, EquatableList<DiagnosticInfo> DiagnosticInfo) where T : IEquatable<T>?;

internal static class ResultExtensions {
    public static Result<T> Result<T>(this T value, params DiagnosticInfo[] diagnosticInfo) where T : IEquatable<T>? {
        return new Result<T>(value, diagnosticInfo.ToEquatableList());
    }

    public static Result<T> Result<T>(this T value, IEnumerable<DiagnosticInfo> diagnosticInfo) where T : IEquatable<T>? {
        return new Result<T>(value, diagnosticInfo.ToEquatableList());
    }
}