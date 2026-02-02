using Phx.Inject.Generator.Incremental.Metadata.Attributes;

namespace Phx.Inject.Generator.Incremental.Model;

internal interface IAttributeElement : ISourceCodeElement {
    /// <summary> Gets the generic Attribute Metadata of the element. </summary>
    AttributeMetadata AttributeMetadata { get; }
}