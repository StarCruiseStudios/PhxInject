// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed [PhxInject] attribute.
/// </summary>
/// <param name="TabSize"> The optional number of spaces for indentation. </param>
/// <param name="GeneratedFileExtension"> The optional file extension for generated files. </param>
/// <param name="NullableEnabled"> The optional nullable reference types setting. </param>
/// <param name="AttributeMetadata"> The underlying attribute metadata. </param>
internal record PhxInjectAttributeMetadata(
    int? TabSize,
    string? GeneratedFileExtension,
    bool? NullableEnabled,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName  =
        $"{PhxInject.NamespaceName}.{nameof(PhxInjectAttribute)}";

    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}