namespace Phx.Inject.Generator.Injectors.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;

    internal record InjectorChildExternalDependencyConstructorArgumentTemplate(
            string ArgumentName,
            string ExternalDependencyImplementationTypeQualifiedName,
            string SpecContainerCollectionReferenceName,
            Location Location
    ) : IInjectorChildConstructorArgumentTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"{ArgumentName}: new {ExternalDependencyImplementationTypeQualifiedName}({SpecContainerCollectionReferenceName})");
        }
    }
}
