//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.5.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Specification {
    internal class GeneratedRawInjector_LeafSpecification {
        private Phx.Inject.Tests.Data.Model.IntLeaf? intLeaf;

        internal Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedRawInjector.SpecContainerCollection specContainers
        ) {
            return intLeaf ??= Phx.Inject.Tests.Data.Specification.LeafSpecification.GetIntLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.StringLeaf GetStringLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedRawInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LeafSpecification.GetStringLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.ParametricLeaf<System.Int32> GetParametricLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedRawInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LeafSpecification.GetParametricLeaf();
        }
    }
}
#nullable restore
