//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.5.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal class GeneratedConstructedInjector_GeneratedConstructedInjector_ConstructorFactories {
        private Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode? autoTypeWithFabricationMode;

        internal Phx.Inject.Tests.Data.Model.AutoType GetConstructorautoType(
                Phx.Inject.Tests.Data.Inject.GeneratedConstructedInjector.SpecContainerCollection specContainers
        ) {
            return new Phx.Inject.Tests.Data.Model.AutoType(
                specContainers.GeneratedConstructedInjector_NonConstructedSpecification.GetIntLeaf(specContainers),
                specContainers.GeneratedConstructedInjector_GeneratedConstructedInjector_ConstructorFactories.GetConstructorautoTypeWithFabricationMode(specContainers));
        }

        internal Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode GetConstructorautoTypeWithFabricationMode(
                Phx.Inject.Tests.Data.Inject.GeneratedConstructedInjector.SpecContainerCollection specContainers
        ) {
            return autoTypeWithFabricationMode ??= new Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode();
        }
    }
}
#nullable restore
