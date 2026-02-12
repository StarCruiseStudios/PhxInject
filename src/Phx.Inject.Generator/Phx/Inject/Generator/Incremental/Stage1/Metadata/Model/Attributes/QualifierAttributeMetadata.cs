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
///     Metadata representing an analyzed [Qualifier] attribute.
/// </summary>
/// <param name="QualifierType"> The type metadata of the qualifier. </param>
/// <param name="AttributeMetadata"> The underlying attribute metadata. </param>
internal record QualifierAttributeMetadata(
    TypeMetadata QualifierType,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    /// <summary> The fully-qualified name of the Qualifier attribute class. </summary>
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(QualifierAttribute)}";
    
    /// <summary> Gets the source location of the attribute. </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}