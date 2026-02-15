// -----------------------------------------------------------------------------
// <copyright file="SpecificationAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Generator.Incremental.PhxInject;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata for the user-declared <c>[Specification]</c> attribute.
///     See <see cref="Phx.Inject.SpecificationAttribute"/>.
/// </summary>
/// <param name="AttributeMetadata">The common attribute metadata shared by all attributes.</param>
/// <remarks>
///     Marks a class or interface as a DI binding specification containing factory methods,
///     builder methods, and link declarations. Specifications are the "recipe book" that
///     injectors consult when resolving dependencies.
/// </remarks>
internal record SpecificationAttributeMetadata(
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{NamespaceName}.{nameof(SpecificationAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}