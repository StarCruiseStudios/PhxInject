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
    internal partial class GeneratedRuntimeFactoryInjector : Phx.Inject.Tests.Data.Inject.IRuntimeFactoryInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.Specification.GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification);

        private readonly SpecContainerCollection specContainers;

        public GeneratedRuntimeFactoryInjector() {
            specContainers = new SpecContainerCollection(
                    GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification: new Phx.Inject.Tests.Data.Specification.GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification());
        }

        public Phx.Inject.Tests.Data.Model.LeafFactory GetLeafFactory() {
            return specContainers.GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification.GetLeafFactory(specContainers);
        }

        public Phx.Inject.Factory<Phx.Inject.Tests.Data.Model.ILeaf> GetLeafRuntimeFactory() {
            return new Phx.Inject.Factory<Phx.Inject.Tests.Data.Model.ILeaf>(() => specContainers.GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification.GetLeaf(specContainers));
        }

        public Phx.Inject.Tests.Data.Model.LeafFactory GetLabeledLeafFactory() {
            return specContainers.GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification.GetLabeledLeafFactory(specContainers);
        }

        public Phx.Inject.Factory<Phx.Inject.Tests.Data.Model.ILeaf> GetLabeledLeafRuntimeFactory() {
            return new Phx.Inject.Factory<Phx.Inject.Tests.Data.Model.ILeaf>(() => specContainers.GeneratedRuntimeFactoryInjector_RuntimeFactorySpecification.GetLabeledLeaf(specContainers));
        }
    }
}
#nullable restore
