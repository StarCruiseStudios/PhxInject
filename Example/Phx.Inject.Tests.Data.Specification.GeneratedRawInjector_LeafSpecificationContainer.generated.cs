//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.0.1.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Specification {
    internal class GeneratedRawInjector_LeafSpecificationContainer {
        private Phx.Inject.Tests.Data.Model.IntLeaf? intLeafInstance;

        internal Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf(
        Phx.Inject.Tests.Data.Inject.GeneratedRawInjector.SpecContainerCollection specContainers) {
            return intLeafInstance ??= Phx.Inject.Tests.Data.Specification.LeafSpecification.GetIntLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.StringLeaf GetStringLeaf(
        Phx.Inject.Tests.Data.Inject.GeneratedRawInjector.SpecContainerCollection specContainers) {
            return Phx.Inject.Tests.Data.Specification.LeafSpecification.GetStringLeaf();
        }
    }
}
#nullable restore
