﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests {
    internal class GeneratedChildInjector_ChildSpecification {

        internal GeneratedChildInjector_ChildSpecification CreateNewFrame() {
            return this;
        }

        internal Phx.Inject.Tests.Data.Model.Node GetNode(
                Phx.Inject.Tests.GeneratedChildInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.ChildSpecification.GetNode(
                specContainers.GeneratedChildInjector_IChildExternalDependencies.GetLeftLeaf(specContainers),
                specContainers.GeneratedChildInjector_IChildExternalDependencies.GetRightLeaf(specContainers));
        }
    }
}
#nullable restore
