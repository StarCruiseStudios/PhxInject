// -----------------------------------------------------------------------------
// <copyright file="AttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

sealed internal record AttributeMetadata(
    string AttributeClassName,
    string TargetName,
    GeneratorIgnored<LocationInfo?> TargetLocation,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {
    public static AttributeMetadata Create(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadata(
            attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName(),
            targetSymbol.ToString(),
            targetSymbol.Locations.FirstOrDefault().GeneratorIgnored(),
            attributeData.GetAttributeLocation(targetSymbol).GeneratorIgnored());
    }
}