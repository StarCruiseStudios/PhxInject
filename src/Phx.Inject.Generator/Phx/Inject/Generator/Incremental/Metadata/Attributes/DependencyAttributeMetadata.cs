using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record DependencyAttributeMetadata(
    TypeModel DependencyType,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public SourceLocation Location { get; } = AttributeMetadata.Location;
}