// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for the user-declared <c>[Label]</c> attribute.
///     See <see cref="Phx.Inject.LabelAttribute"/>.
/// </summary>
/// <param name="Label">The string identifier distinguishing this binding (case-sensitive).</param>
/// <param name="AttributeMetadata">The common attribute metadata shared by all attributes.</param>
/// <remarks>
///     Provides string-based qualification for disambiguating multiple bindings of the same type.
///     When applied to factory/builder, marks what it provides. When applied to parameter, specifies
///     which labeled binding to inject.
/// </remarks>
internal record LabelAttributeMetadata(
    string Label,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(LabelAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}