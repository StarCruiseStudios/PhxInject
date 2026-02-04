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