//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.5.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class GeneratedPropertyFactoryInjector : Phx.Inject.Tests.Data.Inject.IPropertyFactoryInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.Specification.GeneratedPropertyFactoryInjector_IPropertyFactorySpec GeneratedPropertyFactoryInjector_IPropertyFactorySpec,
                Phx.Inject.Tests.Data.Specification.GeneratedPropertyFactoryInjector_PropertyFactoryStaticSpec GeneratedPropertyFactoryInjector_PropertyFactoryStaticSpec);

        private readonly SpecContainerCollection specContainers;

        public GeneratedPropertyFactoryInjector(
                Phx.Inject.Tests.Data.Specification.IPropertyFactorySpec iPropertyFactorySpec
        ) {
            specContainers = new SpecContainerCollection(
                    GeneratedPropertyFactoryInjector_IPropertyFactorySpec: new Phx.Inject.Tests.Data.Specification.GeneratedPropertyFactoryInjector_IPropertyFactorySpec(iPropertyFactorySpec),
                    GeneratedPropertyFactoryInjector_PropertyFactoryStaticSpec: new Phx.Inject.Tests.Data.Specification.GeneratedPropertyFactoryInjector_PropertyFactoryStaticSpec());
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetLeaf() {
            return specContainers.GeneratedPropertyFactoryInjector_IPropertyFactorySpec.GetPropertyLeaf(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.StringLeaf GetStringLeaf() {
            return specContainers.GeneratedPropertyFactoryInjector_PropertyFactoryStaticSpec.GetPropertyStringLeaf(specContainers);
        }
    }
}
#nullable restore
