using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Specification;

internal record SpecFactoryMethodMetadata(
    string FactoryMethodName,
    QualifiedTypeMetadata FactoryReturnType,
    IEnumerable<QualifiedTypeMetadata> Parameters,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    PartialAttributeMetadata? PartialFactoryAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }