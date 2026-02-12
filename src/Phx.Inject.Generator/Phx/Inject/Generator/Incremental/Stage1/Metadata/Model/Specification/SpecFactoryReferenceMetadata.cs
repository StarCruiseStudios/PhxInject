// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryReferenceMetadata.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;

/// <summary>
///     Metadata representing an analyzed specification factory reference.
/// </summary>
/// <param name="FactoryReferenceName"> The name of the factory reference. </param>
/// <param name="FactoryReturnType"> The qualified type returned by the factory. </param>
/// <param name="Parameters"> The list of parameters required by the factory. </param>
/// <param name="FactoryReferenceAttributeMetadata"> The [FactoryReference] attribute metadata. </param>
/// <param name="PartialFactoryAttributeMetadata"> The optional [Partial] attribute metadata. </param>
/// <param name="Location"> The source location of the reference definition. </param>
internal record SpecFactoryReferenceMetadata(
    string FactoryReferenceName,
    QualifiedTypeMetadata FactoryReturnType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    FactoryReferenceAttributeMetadata FactoryReferenceAttributeMetadata,
    PartialAttributeMetadata? PartialFactoryAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
