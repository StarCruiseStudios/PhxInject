﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.9.1.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests {
    internal class GeneratedNestedSpecInjector_OuterNestedSpecification {

        internal GeneratedNestedSpecInjector_OuterNestedSpecification CreateNewFrame() {
            return this;
        }

        internal Phx.Inject.Tests.Data.Model.IntLeaf Fac_IntLeaf_GetIntLeaf(
                Phx.Inject.Tests.GeneratedNestedSpecInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.OuterNestedSpecification.GetIntLeaf(
                specContainers.GeneratedNestedSpecInjector_OuterNestedSpecification_Inner.Fac_Int32_GetIntValue(specContainers));
        }
    }
}
#nullable restore
