using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal interface IAttributeElement : ISourceCodeElement {
    /// <summary> Gets the generic Attribute Metadata of the element. </summary>
    AttributeMetadata AttributeMetadata { get; }
}