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
    internal class GeneratedLabelInjector_LabeledLeafSpecification {

        internal Phx.Inject.Tests.Data.Model.ILeaf GetDefaultLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetDefaultLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetNonDefaultLeafA(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetNonDefaultLeafA();
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetNonDefaultLeafB(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetNonDefaultLeafB();
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetAttributeNamedLeafA(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetAttributeNamedLeafA();
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetAttributeNamedLeafB(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetAttributeNamedLeafB();
        }

        internal Phx.Inject.Tests.Data.Model.ILeaf GetStringNamedLeafA(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetStringNamedLeafA();
        }

        internal Phx.Inject.Tests.Data.Model.StringLeaf GetNamedStringLeaf(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetNamedStringLeaf();
        }

        internal Phx.Inject.Tests.Data.Model.Node GetNode(
                Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.GetNode(
                specContainers.GeneratedLabelInjector_LabeledLeafSpecification.GetNonDefaultLeafA(specContainers),
                specContainers.GeneratedLabelInjector_LabeledLeafSpecification.GetAttributeNamedLeafA(specContainers));
        }

        internal void BuildLazyTypeLeafA(
        Phx.Inject.Tests.Data.Model.LazyType target, Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers) {
            Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.BuildLazyTypeLeafA(
                target,
                specContainers.GeneratedLabelInjector_LabeledLeafSpecification.GetNonDefaultLeafA(specContainers));
        }

        internal void BuildLazyTypeDefault(
        Phx.Inject.Tests.Data.Model.LazyType target, Phx.Inject.Tests.Data.Inject.GeneratedLabelInjector.SpecContainerCollection specContainers) {
            Phx.Inject.Tests.Data.Specification.LabeledLeafSpecification.BuildLazyTypeDefault(
                target,
                specContainers.GeneratedLabelInjector_LabeledLeafSpecification.GetDefaultLeaf(specContainers));
        }
    }
}
#nullable restore
