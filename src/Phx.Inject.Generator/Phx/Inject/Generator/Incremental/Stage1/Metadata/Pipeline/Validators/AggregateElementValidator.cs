// -----------------------------------------------------------------------------
// <copyright file="AggregateElementValidator.cs" company="Star Cruise Studios LLC">
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
///     Combines multiple validators into a single validator using AND logic for symbols and OR logic for syntax.
/// </summary>
/// <param name="validators">
///     Collection of validators to combine. Empty collection produces a validator that always returns 
///     <see langword="true" /> for symbols and <see langword="false" /> for syntax.
/// </param>
/// <remarks>
///     Symbol validation uses AND logic (all constraints must pass). Syntax validation uses OR logic
///     (any validator passing allows proceeding to symbol phase). This enables compositional validation
///     separating orthogonal concerns.
/// </remarks>
internal sealed class AggregateElementValidator(
    IReadOnlyList<ICodeElementValidator> validators
) : ICodeElementValidator {
    /// <inheritdoc />
    public bool IsValidSymbol([NotNullWhen(true)] ISymbol? symbol) {
        return validators.All(validator => validator.IsValidSymbol(symbol));
    }

    /// <inheritdoc />
    public bool IsValidSyntax(SyntaxNode syntaxNode) {
        return validators.Any(validator => validator.IsValidSyntax(syntaxNode));
    }
}