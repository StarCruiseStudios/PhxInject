// -----------------------------------------------------------------------------
//  <copyright file="Diagnostics.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using Microsoft.CodeAnalysis;

    internal static class Diagnostics {
        private const string InjectionCategory = "Injection";
        private const string PhxInjectIdPrefix = "PHXINJECT";

        private const string DebugMessageId = PhxInjectIdPrefix + "0000";

        public static readonly DiagnosticData UnexpectedError = new(
            PhxInjectIdPrefix + "0001",
            "An unexpected error occurred.",
            InjectionCategory);

        public static readonly DiagnosticData InternalError = new(
            PhxInjectIdPrefix + "0002",
            "An internal error occurred while generating injection.",
            InjectionCategory);

        public static readonly DiagnosticData IncompleteSpecification = new(
            PhxInjectIdPrefix + "0003",
            "The provided injection specification is incomplete.",
            InjectionCategory);

        public static readonly DiagnosticData InvalidSpecification = new(
            PhxInjectIdPrefix + "0004",
            "The provided injection specification is invalid.",
            InjectionCategory);

        internal record DiagnosticData(string Id, string Title, string Category);

        public static GeneratorExecutionContext? GeneratorExecutionContext = null;

        public static Diagnostic CreateUnexpectedErrorDiagnostic(string message) =>
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    UnexpectedError.Id,
                    UnexpectedError.Title,
                    message,
                    UnexpectedError.Category,
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None);

        public static void Log(string message, Location? location) =>
            GeneratorExecutionContext?.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                    id: DebugMessageId,
                    title: "Debug message",
                    messageFormat: message,
                    category: InjectionCategory,
                    DiagnosticSeverity.Info,
                    isEnabledByDefault: true
                ),
                location));
    }
}
