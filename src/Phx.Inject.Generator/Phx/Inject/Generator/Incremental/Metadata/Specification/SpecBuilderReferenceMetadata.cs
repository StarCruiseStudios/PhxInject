using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Specification;

internal record SpecBuilderReferenceMetadata(
    string BuilderReferenceName,
    QualifiedTypeMetadata BuiltType,
    IEnumerable<QualifiedTypeMetadata> Parameters,
    BuilderReferenceAttributeMetadata BuilderReferenceAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }