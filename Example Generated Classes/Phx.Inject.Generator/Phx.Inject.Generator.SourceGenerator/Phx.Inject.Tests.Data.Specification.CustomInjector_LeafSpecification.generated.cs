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
    internal class CustomInjector_LeafSpecification {
        private Phx.Inject.Tests.Data.Model.IntLeaf? intLeaf;

        internal CustomInjector_LeafSpecification CreateNewFrame() {
            var newFrame = new CustomInjector_LeafSpecification();
            newFrame.intLeaf = this.intLeaf;
            return newFrame;
        }

        internal Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf(
                Phx.Inject.Tests.Data.Inject.CustomInjector.SpecContainerCollection specContainers
        ) {
            return intLeaf ??= Phx.Inject.Tests.Data.Specification.LeafSpecification.GetIntLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.StringLeaf GetStringLeaf(
                Phx.Inject.Tests.Data.Inject.CustomInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LeafSpecification.GetStringLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.ParametricLeaf<System.Int32> GetParametricLeaf(
                Phx.Inject.Tests.Data.Inject.CustomInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LeafSpecification.GetParametricLeaf();
        }
    }
}
#nullable restore
