// -----------------------------------------------------------------------------
// <copyright file="LinkAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed [Link] attribute.
/// </summary>
/// <param name="Input"> The input type metadata. </param>
/// <param name="Output"> The output type metadata. </param>
/// <param name="InputLabel"> The optional label for the input type. </param>
/// <param name="InputQualifier"> The optional qualifier type for the input. </param>
/// <param name="OutputLabel"> The optional label for the output type. </param>
/// <param name="OutputQualifier"> The optional qualifier type for the output. </param>
/// <param name="AttributeMetadata"> The underlying attribute metadata. </param>
internal record LinkAttributeMetadata(
    TypeMetadata Input,
    TypeMetadata Output,
    string? InputLabel,
    TypeMetadata? InputQualifier,
    string? OutputLabel,
    TypeMetadata? OutputQualifier,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    /// <summary> The fully-qualified name of the Link attribute class. </summary>
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(LinkAttribute)}";
    
    /// <summary> Gets the source location of the attribute. </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}