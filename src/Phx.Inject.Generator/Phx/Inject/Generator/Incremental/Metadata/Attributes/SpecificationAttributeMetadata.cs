using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record SpecificationAttributeMetadata(
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public SourceLocation Location { get; } = AttributeMetadata.Location;
}