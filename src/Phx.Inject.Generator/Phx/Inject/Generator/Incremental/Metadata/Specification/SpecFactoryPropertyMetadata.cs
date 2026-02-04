using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Specification;

internal record SpecFactoryPropertyMetadata(
    string FactoryPropertyName,
    QualifiedTypeMetadata FactoryReturnType,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    PartialAttributeMetadata? PartialFactoryAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }