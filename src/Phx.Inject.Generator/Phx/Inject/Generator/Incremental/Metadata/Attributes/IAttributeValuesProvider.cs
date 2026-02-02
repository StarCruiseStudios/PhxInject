using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal interface IAttributeValuesProvider<out T> where T : IAttributeElement {
    string AttributeClassName { get; }
    bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken);
    T Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken);
}

internal static class AttributeSyntaxProviderExtensions {
    public static IncrementalValuesProvider<T> ForAttribute<T>(
        this SyntaxValueProvider syntaxProvider,
        IAttributeValuesProvider<T> valuesProvider
    ) where T : IAttributeElement {
        return syntaxProvider.ForAttributeWithMetadataName(
            valuesProvider.AttributeClassName,
            valuesProvider.CanProvide,
            valuesProvider.Transform);
    }
}
