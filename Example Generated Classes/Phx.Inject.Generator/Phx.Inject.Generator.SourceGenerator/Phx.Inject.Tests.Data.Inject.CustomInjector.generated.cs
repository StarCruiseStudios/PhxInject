﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class CustomInjector : Phx.Inject.Tests.Data.Inject.ITestInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.Specification.CustomInjector_RootSpecification CustomInjector_RootSpecification,
                Phx.Inject.Tests.Data.Specification.CustomInjector_LazySpecification CustomInjector_LazySpecification,
                Phx.Inject.Tests.Data.Specification.CustomInjector_LeafSpecification CustomInjector_LeafSpecification,
                Phx.Inject.Tests.Data.Specification.CustomInjector_LeafLinks CustomInjector_LeafLinks);

        private readonly SpecContainerCollection specContainers;

        public CustomInjector() {
            specContainers = new SpecContainerCollection(
                    CustomInjector_RootSpecification: new Phx.Inject.Tests.Data.Specification.CustomInjector_RootSpecification(),
                    CustomInjector_LazySpecification: new Phx.Inject.Tests.Data.Specification.CustomInjector_LazySpecification(),
                    CustomInjector_LeafSpecification: new Phx.Inject.Tests.Data.Specification.CustomInjector_LeafSpecification(),
                    CustomInjector_LeafLinks: new Phx.Inject.Tests.Data.Specification.CustomInjector_LeafLinks());
        }

        public Phx.Inject.Tests.Data.Model.Root GetRoot() {
            return specContainers.CustomInjector_RootSpecification.GetRoot(specContainers);
        }

        public void Build(Phx.Inject.Tests.Data.Model.LazyType target) {
            specContainers.CustomInjector_LazySpecification.BuildLazyType(target, specContainers);
        }
    }
}
#nullable restore
