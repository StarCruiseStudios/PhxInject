//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.4.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class GeneratedGrandchildInjector : Phx.Inject.Tests.Data.Inject.IGrandchildInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.Specification.GeneratedGrandchildInjector_GrandchildSpecification GeneratedGrandchildInjector_GrandchildSpecification,
                Phx.Inject.Tests.Data.External.GeneratedGrandchildInjector_IGrandchildExternalDependencies GeneratedGrandchildInjector_IGrandchildExternalDependencies);

        private readonly SpecContainerCollection specContainers;

        public GeneratedGrandchildInjector(
                Phx.Inject.Tests.Data.External.IGrandchildExternalDependencies iGrandchildExternalDependencies
        ) {
            specContainers = new SpecContainerCollection(
                    GeneratedGrandchildInjector_GrandchildSpecification: new Phx.Inject.Tests.Data.Specification.GeneratedGrandchildInjector_GrandchildSpecification(),
                    GeneratedGrandchildInjector_IGrandchildExternalDependencies: new Phx.Inject.Tests.Data.External.GeneratedGrandchildInjector_IGrandchildExternalDependencies(iGrandchildExternalDependencies));
        }

        public Phx.Inject.Tests.Data.Model.Root GetRoot() {
            return specContainers.GeneratedGrandchildInjector_GrandchildSpecification.GetRoot(specContainers);
        }
    }
}
#nullable restore
