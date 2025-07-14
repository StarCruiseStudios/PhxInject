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

        public AggregateInjectionException AsAggregateException(
            string message,
            IReadOnlyList<InjectionException> exceptions,
            IGeneratorContext currentCtx
        ) {
            var msg = string.Join("\n -> ", exceptions.Select(it => it.Message));
            return new AggregateInjectionException(
                message,
                CreateDiagnostic(
                    message + "\n -> " + msg,
                    null,
                    currentCtx.GetLocation()),
                exceptions,
                currentCtx);
        }
    }

    internal sealed class FatalDiagnosticData : DiagnosticData {
        public FatalDiagnosticData(
            string Id,
            string Title,
            string Category,
            DiagnosticSeverity Severity
        ) : base(Id, Title, Category, Severity) { }

        public override InjectionException AsException(
            string message,
            Location location,
            IGeneratorContext currentCtx) {
            return AsFatalException(message, location, currentCtx);
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

        public virtual InjectionException AsException(
            string message,
            Location location,
            IGeneratorContext currentCtx
        ) {
            return new InjectionException(
                message,
                CreateDiagnostic(
                    message,
                    currentCtx.GetFrame(),
                    location),
                currentCtx);
        }

        public virtual InjectionException AsFatalException(
            string message,
            Location location,
            IGeneratorContext currentCtx
        ) {
            return new FatalInjectionException(
                message,
                CreateDiagnostic(
                    message,
                    currentCtx.GetFrame(),
                    location),
                currentCtx);
        }

        public void AsWarning(
            string message,
            Location location,
            IGeneratorContext currentCtx,
            bool includeFrame = false
        ) {
            currentCtx.ExecutionContext.ReportDiagnostic(
                CreateDiagnostic(
                    message,
                    includeFrame ? currentCtx.GetFrame() : null,
                    location));
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
        this IGeneratorContext currentCtx,
        string message,
        Location? location = null
    ) {
        currentCtx.ExecutionContext.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    Diagnostics.DebugMessage.Id,
                    Diagnostics.DebugMessage.Title,
                    message,
                    Diagnostics.DebugMessage.Category,
                    Diagnostics.DebugMessage.Severity,
                    true),
                location ?? currentCtx.GetLocation()));
    }
}
