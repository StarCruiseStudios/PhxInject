//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.1.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal partial class GeneratedParentInjector : Phx.Inject.Tests.Data.Inject.IParentInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.Specification.GeneratedParentInjector_ParentSpecification GeneratedParentInjector_ParentSpecification);

        private readonly SpecContainerCollection specContainers;

        public GeneratedParentInjector() {
            specContainers = new SpecContainerCollection(
                    GeneratedParentInjector_ParentSpecification: new Phx.Inject.Tests.Data.Specification.GeneratedParentInjector_ParentSpecification());
        }

        public Phx.Inject.Tests.Data.Inject.IChildInjector GetChildInjector() {
            return new Phx.Inject.Tests.Data.Inject.GeneratedChildInjector(
                    new Phx.Inject.Tests.Data.Inject.GeneratedParentInjector_IChildExternalDependencies(specContainers));
        }
    }
}
#nullable restore
