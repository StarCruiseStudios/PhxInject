// -----------------------------------------------------------------------------
// <copyright file="PartialAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata representing an analyzed [Partial] attribute.
/// </summary>
/// <param name="AttributeMetadata"> The underlying attribute metadata. </param>
internal record PartialAttributeMetadata(
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(PartialAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}