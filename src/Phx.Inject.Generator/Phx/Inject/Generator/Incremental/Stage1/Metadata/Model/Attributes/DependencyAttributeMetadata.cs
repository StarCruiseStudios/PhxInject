// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata for the user-declared <c>[Dependency]</c> attribute.
///     See <see cref="Phx.Inject.DependencyAttribute"/>.
/// </summary>
/// <param name="DependencyType">The type metadata of the dependency being declared.</param>
/// <param name="AttributeMetadata">The common attribute metadata shared by all attributes.</param>
/// <remarks>
///     Explicitly declares a dependency relationship, typically for child injectors linking to
///     parent dependencies via a dependency interface.
/// </remarks>
internal record DependencyAttributeMetadata(
    TypeMetadata DependencyType,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(DependencyAttribute)}";
    
    /// <summary>
    ///     Gets the validator that defines the structural requirements for dependency interface types.
    /// </summary>
    public static readonly ICodeElementValidator ElementValidator =
        InterfaceElementValidator.PublicInterface;
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}