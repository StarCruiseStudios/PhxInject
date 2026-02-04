// -----------------------------------------------------------------------------
// <copyright file="Diagnostics.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Common.Exceptions;

internal static class Diagnostics {
    private const string InjectionCategory = "Injection";
    private const string PhxInjectIdPrefix = "PHXINJECT";

    public static readonly DiagnosticData DebugMessage = new(
        PhxInjectIdPrefix + "9000",
        "Debug",
        InjectionCategory,
        DiagnosticSeverity.Info);

    public static readonly FatalDiagnosticData UnexpectedError = new(
        PhxInjectIdPrefix + "0001",
        "An unexpected error occurred.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    public static readonly FatalDiagnosticData InternalError = new(
        PhxInjectIdPrefix + "0002",
        "An internal error occurred while generating injection.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    public static readonly DiagnosticData IncompleteSpecification = new(
        PhxInjectIdPrefix + "0003",
        "The provided injection specification is incomplete.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    public static readonly DiagnosticData InvalidSpecification = new(
        PhxInjectIdPrefix + "0004",
        "The provided injection specification is invalid.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    public static readonly DiagnosticData UnusedSpecification = new(
        PhxInjectIdPrefix + "0500",
        "One or more specifications were defined but not used in the injection.",
        InjectionCategory,
        DiagnosticSeverity.Warning);

    public static readonly DiagnosticData AutoFactoryWithSpecification = new(
        PhxInjectIdPrefix + "0501",
        "An auto factory type was created when an explicit factory exists in an unused specification.",
        InjectionCategory,
        DiagnosticSeverity.Warning);

    public static readonly AggregateDiagnosticData AggregateError = new(
        PhxInjectIdPrefix + "1000",
        "One or more errors occurred during injection generation.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    internal sealed class AggregateDiagnosticData : DiagnosticData {
        public AggregateDiagnosticData(
            string Id,
            string Title,
            string Category,
            DiagnosticSeverity Severity
        ) : base(Id, Title, Category, Severity) { }
    }

    internal sealed class FatalDiagnosticData : DiagnosticData {
        public FatalDiagnosticData(
            string Id,
            string Title,
            string Category,
            DiagnosticSeverity Severity
        ) : base(Id, Title, Category, Severity) { }
    }

    internal class DiagnosticData {
        public string Id { get; }
        public string Title { get; }
        public string Category { get; }
        public DiagnosticSeverity Severity { get; }

        public DiagnosticData(string Id, string Title, string Category, DiagnosticSeverity Severity) {
            this.Id = Id;
            this.Title = Title;
            this.Category = Category;
            this.Severity = Severity;
        }

        public Diagnostic CreateDiagnostic(
            string message,
            Location? location = null
        ) {
            return Diagnostic.Create(
                new DiagnosticDescriptor(Id, Title, message, Category, Severity, true),
                location.OrNone());
        }
    }
}
