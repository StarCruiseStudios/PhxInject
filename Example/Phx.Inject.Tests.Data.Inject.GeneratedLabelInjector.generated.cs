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
    internal partial class GeneratedLabelInjector : Phx.Inject.Tests.Data.Inject.ILabelInjector {
        internal interface ISpecContainerCollection {
            Phx.Inject.Tests.Data.Specification.GeneratedLabelInjector_LabeledLeafSpecificationContainer GeneratedLabelInjector_LabeledLeafSpecificationContainer { get; }
        }

        internal class SpecContainerCollection: ISpecContainerCollection  {
            public Phx.Inject.Tests.Data.Specification.GeneratedLabelInjector_LabeledLeafSpecificationContainer GeneratedLabelInjector_LabeledLeafSpecificationContainer { get; } = new Phx.Inject.Tests.Data.Specification.GeneratedLabelInjector_LabeledLeafSpecificationContainer();
        }

        private readonly ISpecContainerCollection specContainers = new SpecContainerCollection();

        public Phx.Inject.Tests.Data.Model.ILeaf GetDefaultLeaf() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetDefaultLeaf(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetNonDefaultLeafA() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetNonDefaultLeafA(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetNonDefaultLeafB() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetNonDefaultLeafB(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetAttributeNamedLeafA() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetAttributeNamedLeafA(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetAttributeNamedLeafB() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetAttributeNamedLeafB(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.ILeaf GetStringNamedLeafA() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetStringNamedLeafA(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.StringLeaf GetNamedStringLeaf() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetNamedStringLeaf(specContainers);
        }

        public Phx.Inject.Tests.Data.Model.Node GetNode() {
            return specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.GetNode(specContainers);
        }

        public void BuildLabeledLazyType(Phx.Inject.Tests.Data.Model.LazyType value) {
            specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.BuildLazyTypeLeafA(value, specContainers);
        }

        public void BuildUnlabeledLazyType(Phx.Inject.Tests.Data.Model.LazyType value) {
            specContainers.GeneratedLabelInjector_LabeledLeafSpecificationContainer.BuildLazyTypeDefault(value, specContainers);
        }
    }
}
#nullable restore
