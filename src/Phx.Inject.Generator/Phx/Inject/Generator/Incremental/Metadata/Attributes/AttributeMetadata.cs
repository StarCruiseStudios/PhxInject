// -----------------------------------------------------------------------------
// <copyright file="AttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record AttributeMetadata(
    string AttributeClassName,
    string TargetName,
    SourceLocation TargetLocation,
    SourceLocation Location
) : ISourceCodeElement {
    public static AttributeMetadata Create(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadata(
            attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName(),
            targetSymbol.ToString(),
            targetSymbol.Locations.FirstOrDefault().ToSourceLocation(),
            attributeData.GetAttributeLocation(targetSymbol).ToSourceLocation());
    }
}
