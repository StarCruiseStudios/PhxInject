// -----------------------------------------------------------------------------
// <copyright file="QualifierAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata for the user-declared <c>[Qualifier]</c> attribute.
///     See <see cref="Phx.Inject.QualifierAttribute"/>.
/// </summary>
/// <param name="QualifierType">
///     The type metadata of the custom qualifier attribute being declared. This type
///     can be applied to factories and parameters for type-safe disambiguation.
/// </param>
/// <param name="AttributeMetadata">The common attribute metadata shared by all attributes.</param>
/// <remarks>
///     Enables users to create domain-specific qualifier attributes by marking an attribute
///     class with <c>[Qualifier]</c>. Unlike <c>[Label]</c> which uses strings, custom qualifiers
///     provide compile-time type safety and can carry additional metadata through their own
///     attribute parameters.
/// </remarks>
internal record QualifierAttributeMetadata(
    TypeMetadata QualifierType,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(QualifierAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}