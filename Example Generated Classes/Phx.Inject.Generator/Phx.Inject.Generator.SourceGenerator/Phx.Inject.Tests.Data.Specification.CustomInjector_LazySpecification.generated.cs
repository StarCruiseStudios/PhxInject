﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Specification {
    internal class CustomInjector_LazySpecification {

        internal CustomInjector_LazySpecification CreateNewFrame() {
            return this;
        }

        internal void BuildLazyType(
                Phx.Inject.Tests.Data.Model.LazyType target,
                Phx.Inject.Tests.Data.Inject.CustomInjector.SpecContainerCollection specContainers
        ) {
            Phx.Inject.Tests.Data.Specification.LazySpecification.BuildLazyType(
                target,
                specContainers.CustomInjector_LeafSpecification.GetStringLeaf(specContainers));
        }
    }
}
#nullable restore
