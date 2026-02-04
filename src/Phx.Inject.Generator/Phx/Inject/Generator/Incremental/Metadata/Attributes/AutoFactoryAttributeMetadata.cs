using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record AutoFactoryAttributeMetadata(
    FabricationMode FabricationMode,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public GeneratorIgnored<Location> Location { get; } = AttributeMetadata.Location;
}