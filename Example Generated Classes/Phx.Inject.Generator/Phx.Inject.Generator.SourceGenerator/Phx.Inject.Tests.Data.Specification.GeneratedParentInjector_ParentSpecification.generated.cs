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
    internal class GeneratedParentInjector_ParentSpecification {

        internal GeneratedParentInjector_ParentSpecification CreateNewFrame() {
            var newFrame = new GeneratedParentInjector_ParentSpecification();
            return newFrame;
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetLeftLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedParentInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.ParentSpecification.GetLeftLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetRightLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedParentInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.ParentSpecification.GetRightLeaf();
        }
    }
}
#nullable restore
