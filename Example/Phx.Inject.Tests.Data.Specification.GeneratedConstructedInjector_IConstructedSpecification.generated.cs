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
    internal class GeneratedConstructedInjector_IConstructedSpecification {
        private Phx.Inject.Tests.Data.Specification.IConstructedSpecification instance;

        public GeneratedConstructedInjector_IConstructedSpecification(Phx.Inject.Tests.Data.Specification.IConstructedSpecification instance) {
            this.instance = instance;
        }

        internal System.Int32 GetIntValue(
        Phx.Inject.Tests.Data.Inject.GeneratedConstructedInjector.SpecContainerCollection specContainers) {
            return instance.GetIntValue();
        }

        internal Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf(
        Phx.Inject.Tests.Data.Inject.GeneratedConstructedInjector.SpecContainerCollection specContainers) {
            return instance.GetIntLeaf(
                specContainers.GeneratedConstructedInjector_IConstructedSpecification.GetIntValue(specContainers));
        }
    }
}
#nullable restore
