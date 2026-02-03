using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record FactoryAttributeMetadata(
    FabricationMode FabricationMode,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public SourceLocation Location { get; } = AttributeMetadata.Location;
}