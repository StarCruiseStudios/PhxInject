//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.3.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class GeneratedConstructedParentInjector : Phx.Inject.Tests.Data.Inject.IConstructedParentInjector {
        internal record SpecContainerCollection (
        );

        private readonly SpecContainerCollection specContainers;

        public GeneratedConstructedParentInjector() {
            specContainers = new SpecContainerCollection(
            );
        }

        public Phx.Inject.Tests.Data.Inject.IConstructedInjector GetChildInjector(
                Phx.Inject.Tests.Data.Specification.IConstructedSpecification iConstructedSpecification
        ) {
            return new Phx.Inject.Tests.Data.Inject.GeneratedConstructedInjector(
                    iConstructedSpecification);
        }
    }
}
#nullable restore
