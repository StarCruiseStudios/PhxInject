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
    internal partial class GeneratedContainerInjector : Phx.Inject.Tests.Data.Inject.IContainerInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.Specification.GeneratedContainerInjector_ContainerSpecification GeneratedContainerInjector_ContainerSpecification,
                Phx.Inject.Tests.Data.Inject.GeneratedContainerInjector_GeneratedContainerInjector_ConstructorFactories GeneratedContainerInjector_GeneratedContainerInjector_ConstructorFactories
        ) {
            internal SpecContainerCollection CreateNewFrame() {
                return new SpecContainerCollection(
                        GeneratedContainerInjector_ContainerSpecification.CreateNewFrame(),
                        GeneratedContainerInjector_GeneratedContainerInjector_ConstructorFactories.CreateNewFrame());
            }
        }

        private readonly SpecContainerCollection specContainers;

        public GeneratedContainerInjector() {
            specContainers = new SpecContainerCollection(
                    GeneratedContainerInjector_ContainerSpecification: new Phx.Inject.Tests.Data.Specification.GeneratedContainerInjector_ContainerSpecification(),
                    GeneratedContainerInjector_GeneratedContainerInjector_ConstructorFactories: new Phx.Inject.Tests.Data.Inject.GeneratedContainerInjector_GeneratedContainerInjector_ConstructorFactories());
        }

        public Phx.Inject.Tests.Data.Model.Node GetNode() {
            return specContainers.GeneratedContainerInjector_ContainerSpecification.GetNode(specContainers);
        }
    }
}
#nullable restore
