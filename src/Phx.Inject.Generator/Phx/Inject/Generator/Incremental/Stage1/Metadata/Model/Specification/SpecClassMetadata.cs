// -----------------------------------------------------------------------------
// <copyright file="SpecClassMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed specification class.
/// </summary>
/// <param name="SpecType"> The type metadata of the specification class. </param>
/// <param name="FactoryMethods"> The list of factory methods in the specification. </param>
/// <param name="FactoryProperties"> The list of factory properties in the specification. </param>
/// <param name="FactoryReferences"> The list of factory references in the specification. </param>
/// <param name="BuilderMethods"> The list of builder methods in the specification. </param>
/// <param name="BuilderReferences"> The list of builder references in the specification. </param>
/// <param name="Links"> The list of link attributes in the specification. </param>
/// <param name="SpecAttributeMetadata"> The [Specification] attribute metadata. </param>
/// <param name="Location"> The source location of the class definition. </param>
internal record SpecClassMetadata(
    TypeMetadata SpecType,
    EquatableList<SpecFactoryMethodMetadata> FactoryMethods,
    EquatableList<SpecFactoryPropertyMetadata> FactoryProperties,
    EquatableList<SpecFactoryReferenceMetadata> FactoryReferences,
    EquatableList<SpecBuilderMethodMetadata> BuilderMethods,
    EquatableList<SpecBuilderReferenceMetadata> BuilderReferences,
    EquatableList<LinkAttributeMetadata> Links,
    SpecificationAttributeMetadata SpecAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
