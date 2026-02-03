using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record LinkAttributeMetadata(
    TypeModel Input,
    TypeModel Output,
    string? InputLabel,
    TypeModel? InputQualifier,
    string? OutputLabel,
    TypeModel? OutputQualifier,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public SourceLocation Location { get; } = AttributeMetadata.Location;
}