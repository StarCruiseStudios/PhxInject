using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record LinkAttributeMetadata(
    TypeMetadata Input,
    TypeMetadata Output,
    string? InputLabel,
    TypeMetadata? InputQualifier,
    string? OutputLabel,
    TypeMetadata? OutputQualifier,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public GeneratorIgnored<Location> Location { get; } = AttributeMetadata.Location;
}