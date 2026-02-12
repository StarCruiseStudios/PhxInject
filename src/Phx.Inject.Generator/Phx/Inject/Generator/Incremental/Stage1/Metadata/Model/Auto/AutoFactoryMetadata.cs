// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed auto-generated factory.
/// </summary>
/// <param name="AutoFactoryType"> The qualified type of the auto-generated factory. </param>
/// <param name="Parameters"> The list of parameters required by the factory. </param>
/// <param name="RequiredProperties"> The list of required properties for the factory. </param>
/// <param name="AutoFactoryAttributeMetadata"> The [AutoFactory] attribute metadata. </param>
/// <param name="Location"> The source location of the factory definition. </param>
internal record AutoFactoryMetadata( 
    QualifiedTypeMetadata AutoFactoryType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    EquatableList<AutoFactoryRequiredPropertyMetadata> RequiredProperties,
    AutoFactoryAttributeMetadata AutoFactoryAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }