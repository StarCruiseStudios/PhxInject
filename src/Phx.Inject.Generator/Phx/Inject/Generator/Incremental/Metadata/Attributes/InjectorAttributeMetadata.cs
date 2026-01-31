using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record InjectorAttributeMetadata(
    string? GeneratedClassName,
    //IReadOnlyList<TypeModel> Specifications,
    AttributeMetadata AttributeMetadata
) : ISourceCodeElement {
    public static readonly string AttributeClassName =
        $"{PhxInject.NamespaceName}.{nameof(InjectorAttribute)}";
    
    public SourceLocation Location { get; } = AttributeMetadata.Location;
    
    public interface IValuesProvider {
        bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken);
        InjectorAttributeMetadata Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken);
    }

    public class ValuesProvider : IValuesProvider {
        public static readonly ValuesProvider Instance = new();

        public bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
            // Generator pipeline ensures this is an InjectorAttribute.
            return true;
        }

        public InjectorAttributeMetadata Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken
        ) {
            var attributeData = context.Attributes.First();
            var targetSymbol = context.TargetSymbol;
            var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

            return new InjectorAttributeMetadata(
                attributeData.ConstructorArguments
                    .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                    .Value as string,
                attributeMetadata);
        }
    }
}
