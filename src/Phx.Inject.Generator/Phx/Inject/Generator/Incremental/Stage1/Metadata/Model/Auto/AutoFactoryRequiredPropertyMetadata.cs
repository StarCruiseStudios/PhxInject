// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryRequiredPropertyMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;

/// <summary>
///     Metadata representing a required property for an auto-generated factory.
/// </summary>
/// <param name="RequiredPropertyName"> The name of the required property. </param>
/// <param name="RequiredPropertyType"> The qualified type of the required property. </param>
/// <param name="Location"> The source location of the property definition. </param>
internal record AutoFactoryRequiredPropertyMetadata(
    string RequiredPropertyName,
    QualifiedTypeMetadata RequiredPropertyType,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }