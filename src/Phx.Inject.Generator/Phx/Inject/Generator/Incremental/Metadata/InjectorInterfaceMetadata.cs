using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata;

internal record InjectorInterfaceMetadata(
    TypeModel InjectorInterfaceType,
    InjectorAttributeMetadata AttributeMetadata,
    SourceLocation Location
) : ISourceCodeElement {
    public interface IValuesProvider {
        InjectorInterfaceMetadata Transform(InjectorAttributeMetadata context, CancellationToken cancellationToken);
    }

    public class ValuesProvider : IValuesProvider {
        public static readonly ValuesProvider Instance = new();

        public InjectorInterfaceMetadata Transform(
            InjectorAttributeMetadata attributeMetadata,
            CancellationToken cancellationToken) {
            return new InjectorInterfaceMetadata(
                attributeMetadata.InjectorInterfaceType,
                attributeMetadata, 
                attributeMetadata.AttributeMetadata.TargetLocation);
        }
    }
}