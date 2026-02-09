// -----------------------------------------------------------------------------
// <copyright file="BuilderReferenceAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

internal record BuilderReferenceAttributeMetadata(
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(BuilderReferenceAttribute)}";
    
    public GeneratorIgnored<Location> Location { get; } = AttributeMetadata.Location;
}