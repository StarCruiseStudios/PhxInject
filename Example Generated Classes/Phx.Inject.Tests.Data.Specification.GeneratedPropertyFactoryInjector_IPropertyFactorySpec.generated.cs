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
    internal class GeneratedPropertyFactoryInjector_IPropertyFactorySpec {
        private readonly Phx.Inject.Tests.Data.Specification.IPropertyFactorySpec instance;

        public GeneratedPropertyFactoryInjector_IPropertyFactorySpec(Phx.Inject.Tests.Data.Specification.IPropertyFactorySpec instance) {
            this.instance = instance;
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetPropertyLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedPropertyFactoryInjector.SpecContainerCollection specContainers
        ) {
            return instance.Leaf;
        }
    }
}
#nullable restore
