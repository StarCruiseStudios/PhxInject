// -----------------------------------------------------------------------------
// <copyright file="AttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Common metadata shared by all analyzed framework attributes.
/// </summary>
/// <param name="AttributeClassName">The fully-qualified name of the attribute class.</param>
/// <param name="TargetName">The fully-qualified name of the symbol this attribute is applied to.</param>
/// <param name="TargetLocation">Source location of the attributed symbol, wrapped in GeneratorIgnored.</param>
/// <param name="Location">Source location of the attribute application itself, wrapped in GeneratorIgnored.</param>
/// <remarks>
///     Location properties are wrapped in GeneratorIgnored to exclude from equality,
///     preventing whitespace/formatting changes from invalidating incremental cache.
/// </remarks>
sealed internal record AttributeMetadata(
    string AttributeClassName,
    string TargetName,
    GeneratorIgnored<LocationInfo?> TargetLocation,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {
    /// <summary>
    ///     Factory method to construct AttributeMetadata from Roslyn's symbol and attribute models.
    /// </summary>
    public static AttributeMetadata Create(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadata(
            attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName(),
            targetSymbol.ToString(),
            targetSymbol.Locations.FirstOrDefault().GeneratorIgnored(),
            attributeData.GetAttributeLocation(targetSymbol).GeneratorIgnored());
    }
}