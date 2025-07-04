// -----------------------------------------------------------------------------
//  <copyright file="Diagnostics.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Common;

internal static class Diagnostics {
    private const string InjectionCategory = "Injection";
    private const string PhxInjectIdPrefix = "PHXINJECT";

    public static readonly DiagnosticData DebugMessage = new(
        PhxInjectIdPrefix + "0000",
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

    public static readonly DiagnosticData AggregateError = new(
        PhxInjectIdPrefix + "9999",
        "One or more errors occurred during injection generation.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    internal record DiagnosticData(string Id, string Title, string Category, DiagnosticSeverity Severity) {
        public InjectionException AsException(string message, GeneratorExecutionContext generatorExecutionContext) {
            return AsException(message, Location.None, generatorExecutionContext);
        }

        public InjectionException AsException(
            string message,
            Location location,
            GeneratorExecutionContext generatorExecutionContext) {
            return new InjectionException(
                message,
                Diagnostic.Create(new DiagnosticDescriptor(Id, Title, message, Category, Severity, true), location),
                generatorExecutionContext);
        }

        public Diagnostic CreateDiagnostic(string message, Location? location = null) {
            return Diagnostic.Create(
                new DiagnosticDescriptor(Id, Title, message, Category, Severity, true),
                location ?? Location.None);
        }
    }
}

internal static class GeneratorExecutionContextExtensions {
    public static void Log(
        this GeneratorExecutionContext generatorExecutionContext,
        string message,
        Location? location = null) {
        generatorExecutionContext.ReportDiagnostic(Diagnostics.DebugMessage.CreateDiagnostic(message, location));
    }
}
