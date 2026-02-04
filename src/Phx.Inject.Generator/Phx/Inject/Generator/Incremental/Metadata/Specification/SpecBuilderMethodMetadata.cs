using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Specification;

internal record SpecBuilderMethodMetadata(
    string BuilderMethodName,
    QualifiedTypeMetadata BuiltType,
    IEnumerable<QualifiedTypeMetadata> Parameters,
    BuilderAttributeMetadata BuilderAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }