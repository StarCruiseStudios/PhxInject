// -----------------------------------------------------------------------------
// <copyright file="FactoryAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for the user-declared <c>[Factory]</c> attribute.
///     See <see cref="Phx.Inject.FactoryAttribute"/>.
/// </summary>
/// <param name="FabricationMode">Specifies the lifetime behavior of instances created by this factory.</param>
/// <param name="AttributeMetadata">The common attribute metadata shared by all attributes.</param>
/// <remarks>
///     FabricationMode determines generated code structure: Scoped generates caching fields,
///     Recurrent generates direct invocation.
/// </remarks>
internal record FactoryAttributeMetadata(
    FabricationMode FabricationMode,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(FactoryAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}