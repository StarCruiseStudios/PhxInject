// -----------------------------------------------------------------------------
// <copyright file="ICodeElementValidator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Diagnostics.CodeAnalysis;
 using Microsoft.CodeAnalysis;

 #endregion

 namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

internal interface ICodeElementValidator {
    bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol);
    bool IsValidSyntax(SyntaxNode syntaxNode);
}

internal static class CodeElementValidator {
    public static ICodeElementValidator Of(params ICodeElementValidator[] validators) {
        return new AggregateElementValidator(validators);
    } 
}