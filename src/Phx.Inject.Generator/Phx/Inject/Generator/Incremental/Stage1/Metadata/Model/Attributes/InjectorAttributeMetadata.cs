// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for the user-declared <c>[Injector]</c> attribute.
///     See <see cref="Phx.Inject.InjectorAttribute"/>.
/// </summary>
/// <param name="GeneratedClassName">Optional custom name for the generated injector implementation class.</param>
/// <param name="Specifications">Ordered list of specification types providing dependency graph definitions.</param>
/// <param name="AttributeMetadata">The common attribute metadata shared by all attributes.</param>
/// <remarks>
///     Marks an interface as a DI container access point. The generator creates an implementation
///     class that implements this interface and orchestrates dependency construction based on specifications.
/// </remarks>
internal record InjectorAttributeMetadata(
    string? GeneratedClassName,
    EquatableList<TypeMetadata> Specifications,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName =
        $"{PhxInject.NamespaceName}.{nameof(InjectorAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}