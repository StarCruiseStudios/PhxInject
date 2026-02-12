// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed [AutoFactory] attribute.
/// </summary>
/// <param name="FabricationMode"> The fabrication mode for the auto factory. </param>
/// <param name="AttributeMetadata"> The underlying attribute metadata. </param>
internal record AutoFactoryAttributeMetadata(
    FabricationMode FabricationMode,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    /// <summary> The fully-qualified name of the AutoFactory attribute class. </summary>
    public const string AttributeClassName = $"{NamespaceName}.{nameof(AutoFactoryAttribute)}";
    
    /// <summary> Gets the source location of the attribute. </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}