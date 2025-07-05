// -----------------------------------------------------------------------------
//  <copyright file="Diagnostics.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator;

namespace Phx.Inject.Common.Exceptions;

internal static class Diagnostics {
    private const string InjectionCategory = "Injection";
    private const string PhxInjectIdPrefix = "PHXINJECT";

    public static readonly DiagnosticData DebugMessage = new(
        PhxInjectIdPrefix + "9000",
        "Debug",
        InjectionCategory,
        DiagnosticSeverity.Info);

    public static readonly DiagnosticData UnexpectedError = new(
        PhxInjectIdPrefix + "0001",
        "An unexpected error occurred.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    public static readonly DiagnosticData InternalError = new(
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

        public AggregateInjectionException AsAggregateException(
            string message,
            IReadOnlyList<InjectionException> exceptions,
            IGeneratorContext generatorExecutionContext
        ) {
            return new AggregateInjectionException(
                CreateDiagnostic(
                    message,
                    generatorExecutionContext.GetFrame(),
                    generatorExecutionContext.GetLocation()),
                exceptions,
                generatorExecutionContext);
        }
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

        public InjectionException AsException(
            string message,
            Location location,
            IGeneratorContext generatorExecutionContext
        ) {
            return new InjectionException(
                CreateDiagnostic(
                    message,
                    generatorExecutionContext.GetFrame(),
                    location),
                generatorExecutionContext);
        }

        protected Diagnostic CreateDiagnostic(
            string message,
            IInjectionFrame? frame,
            Location? location = null
        ) {
            return Diagnostic.Create(
                new DiagnosticDescriptor(Id, Title, message + frame.GetInjectionFrameStack(), Category, Severity, true),
                location ?? Location.None);
        }
    }
}

internal static class GeneratorExecutionContextExtensions {
    public static void Log(
        this IGeneratorContext generatorExecutionContext,
        string message,
        Location? location = null
    ) {
        generatorExecutionContext.ExecutionContext.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    Diagnostics.DebugMessage.Id,
                    Diagnostics.DebugMessage.Title,
                    message,
                    Diagnostics.DebugMessage.Category,
                    Diagnostics.DebugMessage.Severity,
                    true),
                location ?? generatorExecutionContext.GetLocation()));
    }
}
