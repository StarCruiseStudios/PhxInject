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

/// <summary>
///     Validates code element symbols and syntax nodes.
/// </summary>
internal interface ICodeElementValidator {
    /// <summary>
    ///     Determines if the symbol is valid for the code element type.
    /// </summary>
    /// <param name="symbol">The symbol to validate.</param>
    /// <returns>True if the symbol is valid; otherwise, false.</returns>
    bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol);
    
    /// <summary>
    ///     Determines if the syntax node is valid for the code element type.
    /// </summary>
    /// <param name="syntaxNode">The syntax node to validate.</param>
    /// <returns>True if the syntax node is valid; otherwise, false.</returns>
    bool IsValidSyntax(SyntaxNode syntaxNode);
}

/// <summary>
///     Factory methods for creating code element validators.
/// </summary>
internal static class CodeElementValidator {
    /// <summary>
    ///     Creates an aggregate validator from multiple validators.
    /// </summary>
    /// <param name="validators">The validators to aggregate.</param>
    /// <returns>An aggregate validator that requires all validators to pass.</returns>
    public static ICodeElementValidator Of(params ICodeElementValidator[] validators) {
        return new AggregateElementValidator(validators);
    } 
}