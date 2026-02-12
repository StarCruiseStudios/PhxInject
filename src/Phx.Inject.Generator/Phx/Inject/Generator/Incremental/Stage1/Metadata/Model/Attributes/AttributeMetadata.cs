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
///     Metadata representing an analyzed attribute from Stage 1.
/// </summary>
/// <param name="AttributeClassName"> The fully-qualified name of the attribute class. </param>
/// <param name="TargetName"> The name of the symbol the attribute is applied to. </param>
/// <param name="TargetLocation"> The source location of the target symbol. </param>
/// <param name="Location"> The source location of the attribute declaration. </param>
sealed internal record AttributeMetadata(
    string AttributeClassName,
    string TargetName,
    GeneratorIgnored<LocationInfo?> TargetLocation,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {
    /// <summary> Creates an AttributeMetadata instance from a symbol and attribute data. </summary>
    /// <param name="targetSymbol"> The symbol the attribute is applied to. </param>
    /// <param name="attributeData"> The attribute data from Roslyn analysis. </param>
    /// <returns> A new AttributeMetadata instance. </returns>
    public static AttributeMetadata Create(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadata(
            attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName(),
            targetSymbol.ToString(),
            targetSymbol.Locations.FirstOrDefault().GeneratorIgnored(),
            attributeData.GetAttributeLocation(targetSymbol).GeneratorIgnored());
    }
}