//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.3.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Specification {
    internal class GeneratedNestedSpecInjector_OuterNestedSpecification {

        internal Phx.Inject.Tests.Data.Model.IntLeaf GetIntLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedNestedSpecInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.OuterNestedSpecification.GetIntLeaf(
                specContainers.GeneratedNestedSpecInjector_OuterNestedSpecification_Inner.GetIntValue(specContainers));
        }
    }
}
#nullable restore
