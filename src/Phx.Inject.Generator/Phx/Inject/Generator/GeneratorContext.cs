// -----------------------------------------------------------------------------
// <copyright file="GeneratorContext.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator;

internal interface IGeneratorContext {
    ISymbol? Symbol { get; }
    IGeneratorContext? ParentContext { get; }
    GeneratorExecutionContext ExecutionContext { get; }
}

internal static class IGeneratorContextExtensions {
    public static IInjectionFrame GetFrame(this IGeneratorContext generatorCtx) {
        return new InjectionFrame(generatorCtx.Symbol, generatorCtx.ParentContext?.GetFrame());
    }

    public static Location GetLocation(this IGeneratorContext generatorCtx) {
        return generatorCtx.Symbol?.Locations.First() ?? Location.None;
    }
}

internal record class GeneratorContext(
    GeneratorExecutionContext ExecutionContext
) : IGeneratorContext {
    public ISymbol? Symbol {
        get => null;
    }
    
    public IGeneratorContext? ParentContext {
        get => null;
    }
}
