// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;

/// <summary>
///     Metadata representing an analyzed auto-generated builder method.
/// </summary>
/// <param name="AutoBuilderMethodName"> The name of the auto-generated builder method. </param>
/// <param name="BuiltType"> The qualified type that is built by the builder. </param>
/// <param name="Parameters"> The list of parameters required for building. </param>
/// <param name="AutoBuilderAttributeMetadata"> The [AutoBuilder] attribute metadata. </param>
/// <param name="Location"> The source location of the builder definition. </param>
internal record AutoBuilderMetadata(
    string AutoBuilderMethodName,
    QualifiedTypeMetadata BuiltType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    AutoBuilderAttributeMetadata AutoBuilderAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }