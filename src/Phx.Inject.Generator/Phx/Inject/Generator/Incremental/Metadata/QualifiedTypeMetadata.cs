using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata;

internal record QualifiedTypeMetadata(
    TypeMetadata TypeMetadata,
    QualifierAttributeMetadata? QualifierAttribute,
    LabelAttributeMetadata? LabelAttribute
) : ISourceCodeElement {
    public GeneratorIgnored<Location> Location => TypeMetadata.Location;
}