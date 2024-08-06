// -----------------------------------------------------------------------------
//  <copyright file="InjectionException.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using Microsoft.CodeAnalysis;

    internal class InjectionException : Exception {
        public Diagnostics.DiagnosticData DiagnosticData { get; }
        public Location Location { get; }

        public InjectionException(
            Diagnostics.DiagnosticData diagnosticData,
            string message,
            Location location
        ) : base(message) {
            DiagnosticData = diagnosticData;
            Location = location;
        }

        public InjectionException(
            Diagnostics.DiagnosticData diagnosticData,
            string message,
            Location location,
            Exception inner
        ) : base(message, inner) {
            DiagnosticData = diagnosticData;
            Location = location;
        }
    }
}
