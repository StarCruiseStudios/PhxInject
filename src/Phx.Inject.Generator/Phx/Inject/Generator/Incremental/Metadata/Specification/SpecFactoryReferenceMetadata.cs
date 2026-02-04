using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Specification;

internal record SpecFactoryReferenceMetadata(
    string FactoryReferenceName,
    QualifiedTypeMetadata FactoryReturnType,
    IEnumerable<QualifiedTypeMetadata> Parameters,
    FactoryReferenceAttributeMetadata FactoryReferenceAttributeMetadata,
    PartialAttributeMetadata? PartialFactoryAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }