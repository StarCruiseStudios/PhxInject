//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.0.1.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class GeneratedConstructedInjector : Phx.Inject.Tests.Data.Inject.IConstructedInjector {
        internal record SpecContainerCollection (
            Phx.Inject.Tests.Data.Specification.GeneratedConstructedInjector_IConstructedSpecification GeneratedConstructedInjector_IConstructedSpecification = new Phx.Inject.Tests.Data.Specification.GeneratedConstructedInjector_IConstructedSpecification());

        private readonly SpecContainerCollection specContainers = new SpecContainerCollection();

        public Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf() {
            return specContainers.GeneratedConstructedInjector_IConstructedSpecification.GetIntLeaf(specContainers);
        }
    }
}
#nullable restore