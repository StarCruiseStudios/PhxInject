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
    internal class GeneratedParentInjector_IChildExternalDependencies : Phx.Inject.Tests.Data.External.IChildExternalDependencies {
        private readonly Phx.Inject.Tests.Data.Inject.GeneratedParentInjector.SpecContainerCollection specContainers;

        public GeneratedParentInjector_IChildExternalDependencies(Phx.Inject.Tests.Data.Inject.GeneratedParentInjector.SpecContainerCollection specContainers) {
            this.specContainers = specContainers;
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetLeftLeaf() {
            return specContainers.GeneratedParentInjector_ParentSpecification.GetLeftLeaf(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetRightLeaf() {
            return specContainers.GeneratedParentInjector_ParentSpecification.GetRightLeaf(specContainers);
        }
    }
}
#nullable restore
