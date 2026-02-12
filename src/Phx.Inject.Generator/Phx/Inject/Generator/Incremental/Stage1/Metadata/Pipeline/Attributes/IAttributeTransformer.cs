// -----------------------------------------------------------------------------
// <copyright file="IAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms attribute data from a symbol into attribute metadata.
/// </summary>
/// <typeparam name="TAttributeMetadata">The type of attribute metadata to produce.</typeparam>
internal interface IAttributeTransformer<out TAttributeMetadata> : IAttributeChecker where TAttributeMetadata : IAttributeElement, IEquatable<TAttributeMetadata> {
    /// <summary>
    ///     Transforms the attribute on the target symbol into metadata.
    /// </summary>
    /// <param name="targetSymbol">The symbol with the attribute.</param>
    /// <returns>A result containing the attribute metadata.</returns>
    IResult<TAttributeMetadata> Transform(ISymbol targetSymbol);
}

/// <summary>
///     Extension methods for attribute transformers.
/// </summary>
internal static class IAttributeTransformerExtensions {
    /// <summary>
    ///     Transforms the attribute if present, or returns null.
    /// </summary>
    /// <typeparam name="TAttributeMetadata">The type of attribute metadata.</typeparam>
    /// <param name="transformer">The attribute transformer.</param>
    /// <param name="targetSymbol">The symbol to check and transform.</param>
    /// <returns>The transform result if the attribute exists; otherwise, null.</returns>
    public static IResult<TAttributeMetadata>? TransformOrNull<TAttributeMetadata>(this IAttributeTransformer<TAttributeMetadata> transformer, ISymbol targetSymbol) where TAttributeMetadata : IAttributeElement, IEquatable<TAttributeMetadata> {
        return transformer.HasAttribute(targetSymbol)
            ? transformer.Transform(targetSymbol)
            : null;
    }
}