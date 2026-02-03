using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata;

internal record InjectorInterfaceMetadata(
    // TypeModel InjectorInterfaceType,
    InjectorAttributeMetadata AttributeMetadata,
    SourceLocation Location
) : ISourceCodeElement { }