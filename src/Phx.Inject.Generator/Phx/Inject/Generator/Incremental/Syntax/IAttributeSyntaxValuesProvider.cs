using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;

namespace Phx.Inject.Generator.Incremental.Syntax;

internal interface IAttributeSyntaxValuesProvider<out T> where T : IAttributeElement {
    string AttributeClassName { get; }
    bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken);
    T Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken);
}

internal static class AttributeSyntaxProviderExtensions {
    public static IncrementalValuesProvider<T> ForAttribute<T>(
        this SyntaxValueProvider syntaxProvider,
        IAttributeSyntaxValuesProvider<T> syntaxValuesProvider
    ) where T : IAttributeElement {
        return syntaxProvider.ForAttributeWithMetadataName(
            syntaxValuesProvider.AttributeClassName,
            syntaxValuesProvider.CanProvide,
            syntaxValuesProvider.Transform);
    }
}
