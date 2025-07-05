// -----------------------------------------------------------------------------
//  <copyright file="InjectionException.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator;

namespace Phx.Inject.Common.Exceptions;

internal class InjectionException : Exception {
    public Diagnostic Diagnostic { get; }

    public InjectionException(
        string message,
        Diagnostic diagnostic,
        IGeneratorContext generatorContext
    ) : base(message) {
        Diagnostic = diagnostic;
        generatorContext.ExecutionContext.ReportDiagnostic(Diagnostic);
    }
}
