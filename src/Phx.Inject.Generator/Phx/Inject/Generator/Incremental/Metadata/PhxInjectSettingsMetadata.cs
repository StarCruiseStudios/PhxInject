// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettings.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata;

/// <summary> Defines the available settings for Phx.Inject source generation. </summary>
/// <param name="Name"> The display name of the settings instance. </param>
/// <param name="TabSize"> The number of spaces to use as indentation in generated source. </param>
/// <param name="GeneratedFileExtension"> The file extension to use for the generated source. </param>
/// <param name="NullableEnabled">
///     A value indicating whether nullable values should be enabled in the generated source.
/// </param>
/// <param name="DisableAutoSpecification">
///     A value indicating whether factories and builders can be automatically generated from specified
///     source. If <c> true </c> only factories, builders, and links defined in a specification will be
///     considered for injection.
/// </param>
/// <param name="AttributeMetadata">
///     The <see cref="PhxInjectAttributeMetadata"/> used to generate the
///     settings.
/// </param>
/// <param name="Location"> The source location of the settings definition. </param>
internal record PhxInjectSettingsMetadata(
    string Name,
    int TabSize,
    string GeneratedFileExtension,
    bool NullableEnabled,
    bool DisableAutoSpecification,
    PhxInjectAttributeMetadata? AttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement {
    /// <summary> Initializes a new instance of the <see cref="PhxInjectSettingsMetadata"/> class. </summary>
    /// <param name="attributeMetadata">
    ///     The <see cref="PhxInjectAttributeMetadata"/> that defines the settings. The default settings
    ///     will be used if <c> null </c>.
    /// </param>
    public PhxInjectSettingsMetadata(
        PhxInjectAttributeMetadata? attributeMetadata
    ) : this(
        attributeMetadata?.AttributeMetadata.TargetName ?? "Default",
        attributeMetadata?.TabSize ?? PhxInjectAttribute.DefaultTabSize,
        attributeMetadata?.GeneratedFileExtension ?? PhxInjectAttribute.DefaultGeneratedFileExtension,
        attributeMetadata?.NullableEnabled ?? PhxInjectAttribute.DefaultNullableEnabled,
        attributeMetadata?.AllowConstructorFactories ?? PhxInjectAttribute.DefaultAllowConstructorFactories,
        attributeMetadata,
        (attributeMetadata?.Location).OrNone()
    ) { }

    public interface IValuesProvider {
        PhxInjectSettingsMetadata Transform(PhxInjectAttributeMetadata context, CancellationToken cancellationToken);
    }

    public class ValuesProvider : IValuesProvider {
        public static readonly ValuesProvider Instance = new();

        public PhxInjectSettingsMetadata Transform(
            PhxInjectAttributeMetadata attributeMetadata,
            CancellationToken cancellationToken) {
            return new PhxInjectSettingsMetadata(attributeMetadata);
        }
    }
}
