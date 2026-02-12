// -----------------------------------------------------------------------------
// <copyright file="IAttributeListTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms multiple attributes from a symbol into a list of attribute metadata.
/// </summary>
/// <typeparam name="TAttributeMetadata">The type of attribute metadata to produce.</typeparam>
internal interface IAttributeListTransformer<TAttributeMetadata> : IAttributeChecker where TAttributeMetadata : IAttributeElement {
    /// <summary>
    ///     Transforms all matching attributes on the target symbol into a list.
    /// </summary>
    /// <param name="targetSymbol">The symbol with the attributes.</param>
    /// <returns>An equatable list of attribute metadata.</returns>
    EquatableList<TAttributeMetadata> Transform(ISymbol targetSymbol);
}