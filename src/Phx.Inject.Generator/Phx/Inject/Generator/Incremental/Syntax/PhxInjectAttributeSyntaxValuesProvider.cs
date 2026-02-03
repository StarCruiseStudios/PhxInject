using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;

namespace Phx.Inject.Generator.Incremental.Syntax;

internal class PhxInjectAttributeSyntaxValuesProvider : IAttributeSyntaxValuesProvider<PhxInjectAttributeMetadata> {
    public static readonly PhxInjectAttributeSyntaxValuesProvider Instance = new();

    public string AttributeClassName  { get; } =
        $"{PhxInject.NamespaceName}.{nameof(PhxInjectAttribute)}";

    public bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        // Generator pipeline ensures this is a PhxInjectAttribute.
        return true;
    }

    public PhxInjectAttributeMetadata Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    ) {
        var attributeData = context.Attributes.First();
        var targetSymbol = context.TargetSymbol;
        var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

        return new PhxInjectAttributeMetadata(
            attributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.TabSize))
                .Value.Value as int?,
            attributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.GeneratedFileExtension))
                .Value.Value as string,
            attributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.NullableEnabled))
                .Value.Value as bool?,
            attributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.AllowConstructorFactories))
                .Value.Value as bool?,
            attributeMetadata);
    }
}