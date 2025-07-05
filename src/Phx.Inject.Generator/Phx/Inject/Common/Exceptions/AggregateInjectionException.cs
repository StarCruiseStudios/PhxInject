// -----------------------------------------------------------------------------
// <copyright file="AggregateInjectionException.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator;

namespace Phx.Inject.Common.Exceptions;

internal sealed class AggregateInjectionException : FatalInjectionException {
    public IReadOnlyList<InjectionException> Exceptions { get; }

    internal AggregateInjectionException(
        string message,
        Diagnostic diagnostic,
        IEnumerable<InjectionException> exceptions,
        IGeneratorContext generatorContext
    ) : base(
        message,
        diagnostic,
        generatorContext
    ) {
        Exceptions = exceptions.ToImmutableList();
    }
}
