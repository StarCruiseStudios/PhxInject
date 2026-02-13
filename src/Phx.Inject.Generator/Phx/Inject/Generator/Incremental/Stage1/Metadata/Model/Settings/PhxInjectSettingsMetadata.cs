// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettingsMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Settings;

/// <summary> Defines the available settings for Phx.Inject source generation. </summary>
/// <param name="Name"> The display name of the settings instance. </param>
/// <param name="TabSize"> The number of spaces to use as indentation in generated source. </param>
/// <param name="GeneratedFileExtension"> The file extension to use for the generated source. </param>
/// <param name="NullableEnabled">
///     A value indicating whether nullable values should be enabled in the generated source.
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
    PhxInjectAttributeMetadata? AttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
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
        attributeMetadata,
        (attributeMetadata?.Location).OrNone()
    ) { }

    /// <summary> Interface for providers that transform PhxInjectAttributeMetadata into settings. </summary>
    public interface IValuesProvider {
        /// <summary> Transforms attribute metadata into settings metadata. </summary>
        /// <param name="context"> The PhxInjectAttributeMetadata to transform. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A new PhxInjectSettingsMetadata instance. </returns>
        PhxInjectSettingsMetadata Transform(PhxInjectAttributeMetadata context, CancellationToken cancellationToken);
    }

    /// <summary> Default implementation of IValuesProvider for transforming settings. </summary>
    public class ValuesProvider : IValuesProvider {
        /// <summary> Gets the singleton instance of the ValuesProvider. </summary>
        public static readonly ValuesProvider Instance = new();

        /// <summary> Transforms attribute metadata into settings metadata. </summary>
        /// <param name="attributeMetadata"> The attribute metadata to transform. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A new PhxInjectSettingsMetadata instance. </returns>
        public PhxInjectSettingsMetadata Transform(
            PhxInjectAttributeMetadata attributeMetadata,
            CancellationToken cancellationToken) {
            return new PhxInjectSettingsMetadata(attributeMetadata);
        }
    }
}
