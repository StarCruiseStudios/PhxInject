//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.0.1.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class GeneratedRawInjector : Phx.Inject.Tests.Data.Inject.IRawInjector {
        internal interface ISpecContainerCollection {
            Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_RootSpecificationContainer GeneratedRawInjector_RootSpecificationContainer { get; }
            Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LazySpecificationContainer GeneratedRawInjector_LazySpecificationContainer { get; }
            Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LeafSpecificationContainer GeneratedRawInjector_LeafSpecificationContainer { get; }
            Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LeafLinksContainer GeneratedRawInjector_LeafLinksContainer { get; }
        }

        internal class SpecContainerCollection: ISpecContainerCollection  {
            public Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_RootSpecificationContainer GeneratedRawInjector_RootSpecificationContainer { get; } = new Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_RootSpecificationContainer();
            public Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LazySpecificationContainer GeneratedRawInjector_LazySpecificationContainer { get; } = new Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LazySpecificationContainer();
            public Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LeafSpecificationContainer GeneratedRawInjector_LeafSpecificationContainer { get; } = new Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LeafSpecificationContainer();
            public Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LeafLinksContainer GeneratedRawInjector_LeafLinksContainer { get; } = new Phx.Inject.Tests.Data.Specification.GeneratedRawInjector_LeafLinksContainer();
        }

        private readonly ISpecContainerCollection specContainers = new SpecContainerCollection();

        public Phx.Inject.Tests.Data.Model.Root GetRoot() {
            return specContainers.GeneratedRawInjector_RootSpecificationContainer.GetRoot(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.Node GetNode() {
            return specContainers.GeneratedRawInjector_RootSpecificationContainer.GetNode(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf() {
            return specContainers.GeneratedRawInjector_LeafSpecificationContainer.GetIntLeaf(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.StringLeaf GetStringLeaf() {
            return specContainers.GeneratedRawInjector_LeafSpecificationContainer.GetStringLeaf(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetILeaf() {
            return specContainers.GeneratedRawInjector_LeafSpecificationContainer.GetStringLeaf(specContainers);
        }

        public void Build(Phx.Inject.Tests.Data.Model.LazyType value) {
            specContainers.GeneratedRawInjector_LazySpecificationContainer.BuildLazyType(value, specContainers);
        }
    }
}
#nullable restore
