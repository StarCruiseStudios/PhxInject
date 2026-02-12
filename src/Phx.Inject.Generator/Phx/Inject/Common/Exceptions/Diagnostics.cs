// -----------------------------------------------------------------------------
// <copyright file="Diagnostics.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Common.Exceptions;

/// <summary>
///     Contains diagnostic data definitions for all errors, warnings, and info messages produced by the generator.
/// </summary>
internal static class Diagnostics {
    /// <summary> The diagnostic category for all injection-related diagnostics. </summary>
    private const string InjectionCategory = "Injection";
    
    /// <summary> The prefix used for all Phx.Inject diagnostic IDs. </summary>
    private const string PhxInjectIdPrefix = "PHXINJECT";

    /// <summary> Debug information diagnostic. </summary>
    public static readonly DiagnosticData DebugMessage = new(
        PhxInjectIdPrefix + "9000",
        "Debug",
        InjectionCategory,
        DiagnosticSeverity.Info);

    /// <summary> Fatal diagnostic for unexpected errors during generation. </summary>
    public static readonly FatalDiagnosticData UnexpectedError = new(
        PhxInjectIdPrefix + "0001",
        "An unexpected error occurred.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    /// <summary> Fatal diagnostic for internal generator errors. </summary>
    public static readonly FatalDiagnosticData InternalError = new(
        PhxInjectIdPrefix + "0002",
        "An internal error occurred while generating injection.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    /// <summary> Error diagnostic for incomplete injection specifications. </summary>
    public static readonly DiagnosticData IncompleteSpecification = new(
        PhxInjectIdPrefix + "0003",
        "The provided injection specification is incomplete.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    /// <summary> Error diagnostic for invalid injection specifications. </summary>
    public static readonly DiagnosticData InvalidSpecification = new(
        PhxInjectIdPrefix + "0004",
        "The provided injection specification is invalid.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    /// <summary> Warning diagnostic for unused specifications. </summary>
    public static readonly DiagnosticData UnusedSpecification = new(
        PhxInjectIdPrefix + "0500",
        "One or more specifications were defined but not used in the injection.",
        InjectionCategory,
        DiagnosticSeverity.Warning);

    /// <summary> Warning diagnostic for when an auto factory is created despite an explicit factory existing in an unused specification. </summary>
    public static readonly DiagnosticData AutoFactoryWithSpecification = new(
        PhxInjectIdPrefix + "0501",
        "An auto factory type was created when an explicit factory exists in an unused specification.",
        InjectionCategory,
        DiagnosticSeverity.Warning);

    /// <summary> Error diagnostic for when multiple errors occurred during generation. </summary>
    public static readonly AggregateDiagnosticData AggregateError = new(
        PhxInjectIdPrefix + "1000",
        "One or more errors occurred during injection generation.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    /// <summary>
    ///     Diagnostic data for aggregate errors that collect multiple child diagnostics.
    /// </summary>
    internal sealed class AggregateDiagnosticData : DiagnosticData {
        public AggregateDiagnosticData(
            string Id,
            string Title,
            string Category,
            DiagnosticSeverity Severity
        ) : base(Id, Title, Category, Severity) { }
    }

    /// <summary>
    ///     Diagnostic data for fatal errors that should halt code generation.
    /// </summary>
    internal sealed class FatalDiagnosticData : DiagnosticData {
        public FatalDiagnosticData(
            string Id,
            string Title,
            string Category,
            DiagnosticSeverity Severity
        ) : base(Id, Title, Category, Severity) { }
    }

    /// <summary>
    ///     Base class containing diagnostic metadata for creating Roslyn diagnostic reports.
    /// </summary>
    internal class DiagnosticData {
        /// <summary> The unique diagnostic identifier. </summary>
        public string Id { get; }
        
        /// <summary> The diagnostic title. </summary>
        public string Title { get; }
        
        /// <summary> The diagnostic category. </summary>
        public string Category { get; }
        
        /// <summary> The diagnostic severity level. </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiagnosticData"/> class.
        /// </summary>
        /// <param name="Id"> The unique diagnostic identifier. </param>
        /// <param name="Title"> The diagnostic title. </param>
        /// <param name="Category"> The diagnostic category. </param>
        /// <param name="Severity"> The diagnostic severity level. </param>
        public DiagnosticData(string Id, string Title, string Category, DiagnosticSeverity Severity) {
            this.Id = Id;
            this.Title = Title;
            this.Category = Category;
            this.Severity = Severity;
        }

        /// <summary>
        ///     Creates a Roslyn <see cref="Diagnostic"/> instance from this diagnostic data.
        /// </summary>
        /// <param name="message"> The diagnostic message. </param>
        /// <param name="location"> The source code location where the diagnostic occurred. </param>
        /// <returns> A new <see cref="Diagnostic"/> instance. </returns>
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
