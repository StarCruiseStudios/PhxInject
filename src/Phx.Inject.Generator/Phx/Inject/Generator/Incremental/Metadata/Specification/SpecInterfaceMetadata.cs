// -----------------------------------------------------------------------------
// <copyright file="SpecInterfaceMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Specification;

internal record SpecInterfaceMetadata(
    TypeMetadata SpecInterfaceType,
    IEnumerable<SpecFactoryMethodMetadata> FactoryMethods,
    IEnumerable<SpecFactoryPropertyMetadata> FactoryProperties,
    IEnumerable<SpecFactoryReferenceMetadata> FactoryReferences,
    IEnumerable<SpecBuilderMethodMetadata> BuilderMethods,
    IEnumerable<SpecBuilderReferenceMetadata> BuilderReferences,
    IEnumerable<LinkAttributeMetadata> Links,
    SpecificationAttributeMetadata SpecAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }