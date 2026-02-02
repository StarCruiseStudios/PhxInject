using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record InjectorAttributeMetadata(
    TypeModel InjectorInterfaceType,
    string? GeneratedClassName,
    IReadOnlyList<TypeModel> Specifications,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public SourceLocation Location { get; } = AttributeMetadata.Location;
    
    public class ValuesProvider : IAttributeValuesProvider<InjectorAttributeMetadata> {
        public static readonly ValuesProvider Instance = new();

        public string AttributeClassName { get; } =
            $"{PhxInject.NamespaceName}.{nameof(InjectorAttribute)}";

        public bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
            // Generator pipeline ensures this is an InjectorAttribute.
            if (syntaxNode is InterfaceDeclarationSyntax { Modifiers: var modifiers }) {
                return modifiers
                    .All(it => it.ValueText switch {
                        "private" or "protected" => false,
                        "internal" or "public" => true,
                        _ => true
                    });
            }
            
            return false;
        }

        public InjectorAttributeMetadata Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken
        ) {
            var attributeData = context.Attributes.First();
            var targetSymbol = (ITypeSymbol)context.TargetSymbol;
            var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

            var generatedClassName = attributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(InjectorAttribute.GeneratedClassName))
                .Value.Value as string
                ?? attributeData.ConstructorArguments
                .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                .Value as string;
            
            var specifications = attributeData.ConstructorArguments
                .Where(argument => argument.Kind != TypedConstantKind.Array)
                .SelectMany(argument => argument.Values)
                .Select(type => type.Value as ITypeSymbol)
                .OfType<ITypeSymbol>()
                .Select(it => it.ToTypeModel())
                .ToImmutableList();
            
            return new InjectorAttributeMetadata(
                targetSymbol.ToTypeModel(),
                generatedClassName,
                specifications,
                attributeMetadata);
        }
    }
}
