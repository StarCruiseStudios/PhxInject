// -----------------------------------------------------------------------------
// <copyright file="SpecBuilderReferenceMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed specification builder reference.
/// </summary>
/// <param name="BuilderReferenceName"> The name of the builder reference. </param>
/// <param name="BuiltType"> The qualified type that is built. </param>
/// <param name="Parameters"> The list of parameters required for building. </param>
/// <param name="BuilderReferenceAttributeMetadata"> The [BuilderReference] attribute metadata. </param>
/// <param name="Location"> The source location of the reference definition. </param>
internal record SpecBuilderReferenceMetadata(
    string BuilderReferenceName,
    QualifiedTypeMetadata BuiltType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    BuilderReferenceAttributeMetadata BuilderReferenceAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
