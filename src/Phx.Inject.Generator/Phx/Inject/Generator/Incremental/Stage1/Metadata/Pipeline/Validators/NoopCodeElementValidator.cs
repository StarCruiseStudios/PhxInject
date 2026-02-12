// -----------------------------------------------------------------------------
// <copyright file="NoopCodeElementValidator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

internal class NoopCodeElementValidator : ICodeElementValidator {
    public static readonly NoopCodeElementValidator Instance = new();
    
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) => true;
    public bool IsValidSyntax(SyntaxNode syntaxNode) => true;
    
    private NoopCodeElementValidator() { }
}