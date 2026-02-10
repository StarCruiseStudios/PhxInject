// -----------------------------------------------------------------------------
// <copyright file="Diagnostics.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Incremental.Diagnostics;

internal class DiagnosticType {
    private const string InjectionCategory = "Injection";
    private const string PhxInjectIdPrefix = "PHXINJECT";
    public static readonly DiagnosticType DebugMessage = new(
        PhxInjectIdPrefix + "9000",
        "Debug",
        InjectionCategory,
        DiagnosticSeverity.Info);
    
    public static readonly DiagnosticType UnexpectedError = new(
        PhxInjectIdPrefix + "0001",
        "An unexpected error occurred.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    public static readonly DiagnosticType InternalError = new(
        PhxInjectIdPrefix + "0002",
        "An internal error occurred while generating injection.",
        InjectionCategory,
        DiagnosticSeverity.Error);
    
    public string Id { get; }
    public string Title { get; }
    public string Category { get; }
    public DiagnosticSeverity Severity { get; }
    public bool IsEnabledByDefault { get; }

    private DiagnosticType(
        string id,
        string title,
        string category,
        DiagnosticSeverity severity,
        bool isEnabledByDefault = true
    ) {
        Id = id;
        Title = title;
        Category = category;
        Severity = severity;
        IsEnabledByDefault = isEnabledByDefault;
    }
        
    public virtual bool Equals(DiagnosticType? other) {
        return other is not null && Id == other.Id;
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }
}