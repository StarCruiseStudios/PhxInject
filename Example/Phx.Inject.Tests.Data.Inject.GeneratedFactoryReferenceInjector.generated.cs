//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.2.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class GeneratedFactoryReferenceInjector : Phx.Inject.Tests.Data.Inject.IFactoryReferenceInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.Specification.GeneratedFactoryReferenceInjector_FactoryReferenceSpec GeneratedFactoryReferenceInjector_FactoryReferenceSpec);

        private readonly SpecContainerCollection specContainers;

        public GeneratedFactoryReferenceInjector() {
            specContainers = new SpecContainerCollection(
                    GeneratedFactoryReferenceInjector_FactoryReferenceSpec: new Phx.Inject.Tests.Data.Specification.GeneratedFactoryReferenceInjector_FactoryReferenceSpec());
        }

        public Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf() {
            return specContainers.GeneratedFactoryReferenceInjector_FactoryReferenceSpec.GetReferenceGetIntLeaf(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.StringLeaf GetStringLeaf() {
            return specContainers.GeneratedFactoryReferenceInjector_FactoryReferenceSpec.GetReferenceGetStringLeaf(specContainers);
        }

        public void Build(Phx.Inject.Tests.Data.Model.LazyType target) {
            specContainers.GeneratedFactoryReferenceInjector_FactoryReferenceSpec.GetReferenceBuildLazyType(target, specContainers);
        }

        public void BuildField(Phx.Inject.Tests.Data.Model.LazyType target) {
            specContainers.GeneratedFactoryReferenceInjector_FactoryReferenceSpec.GetReferenceBuildLazyTypeField(target, specContainers);
        }
    }
}
#nullable restore
