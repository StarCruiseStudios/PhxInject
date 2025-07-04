// -----------------------------------------------------------------------------
//  <copyright file="InjectionException.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common.Exceptions;

internal class InjectionException : Exception {
    public Diagnostic Diagnostic { get; }

    public InjectionException(
        string message,
        Diagnostic diagnostic,
        GeneratorExecutionContext generatorContext
    ) : base(message) {
        Diagnostic = diagnostic;
        generatorContext.ReportDiagnostic(Diagnostic);
    }

    public InjectionException(
        string message,
        Diagnostics.DiagnosticData diagnosticData,
        Location location,
        GeneratorExecutionContext context) :
        this(message, diagnosticData.CreateDiagnostic(message, location), context) { }
}
