// -----------------------------------------------------------------------------
// <copyright file="BuilderAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for the user-declared <c>[Builder]</c> attribute.
///     See <see cref="Phx.Inject.BuilderAttribute"/>.
/// </summary>
/// <param name="AttributeMetadata">The common attribute metadata shared by all attributes.</param>
/// <remarks>
///     Builders initialize existing objects via property/field injection. Must return void
///     and accept the target instance as the first parameter.
/// </remarks>
internal record BuilderAttributeMetadata(
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(BuilderAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}