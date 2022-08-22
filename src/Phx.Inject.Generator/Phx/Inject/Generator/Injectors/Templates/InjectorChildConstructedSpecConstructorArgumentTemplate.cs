namespace Phx.Inject.Generator.Injectors.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;

    internal record InjectorChildConstructedSpecConstructorArgumentTemplate(
            string ArgumentName,
            string SpecParameterName,
            Location Location
    ) : IInjectorChildConstructorArgumentTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"{ArgumentName}: {SpecParameterName}");
        }
    }
}
