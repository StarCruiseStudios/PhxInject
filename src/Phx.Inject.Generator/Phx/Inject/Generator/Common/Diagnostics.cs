// -----------------------------------------------------------------------------
//  <copyright file="Diagnostics.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator {
    internal static class Diagnostics {
        private const string InjectionCategory = "Injection";
        private const string PhxInjectIdPrefix = "PHXINJECT";
        internal record DiagnosticData(string Id, string Title, string Category);

        public static readonly DiagnosticData UnexpectedError = new DiagnosticData(
                PhxInjectIdPrefix + "0001",
                "An unexpected error occurred.",
                InjectionCategory);

        public static readonly DiagnosticData InternalError = new DiagnosticData(
                PhxInjectIdPrefix + "0002",
                "An internal error occurred while generating injection.",
                InjectionCategory);

        public static readonly DiagnosticData IncompleteSpecification = new DiagnosticData(
                PhxInjectIdPrefix + "0003",
                "The provided injection specification is incomplete.",
                InjectionCategory);

        public static readonly DiagnosticData InvalidSpecification = new DiagnosticData(
                PhxInjectIdPrefix + "0004",
                "The provided injection specification is invalid.",
                InjectionCategory);
    }
}
