// -----------------------------------------------------------------------------
//  <copyright file="ExtractionContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Extract;

internal record ExtractorContext : IGeneratorContext {
    public ISymbol? Symbol { get; private init; }
    public IGeneratorContext? ParentContext { get; private init; }
    public GeneratorExecutionContext ExecutionContext { get; }
    
    public ExtractorContext(
        GeneratorExecutionContext executionCtx
    ) {
        Symbol = null;
        ParentContext = null;
        ExecutionContext = executionCtx;
    }

    public ExtractorContext GetChildContext(ISymbol? symbol, Location location) {
        return new ExtractorContext(ExecutionContext) {
            Symbol = symbol,
            ParentContext = this
        };
    }
}
