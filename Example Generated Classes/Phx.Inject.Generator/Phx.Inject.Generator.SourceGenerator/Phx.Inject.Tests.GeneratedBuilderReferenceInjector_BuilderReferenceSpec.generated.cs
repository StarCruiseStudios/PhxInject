﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests {
    internal class GeneratedBuilderReferenceInjector_BuilderReferenceSpec {
        private Phx.Inject.Tests.Data.Model.IntLeaf? intLeaf;

        internal GeneratedBuilderReferenceInjector_BuilderReferenceSpec CreateNewFrame() {
            return this;
        }

        internal System.Int32 GetPropertyIntValue(
                Phx.Inject.Tests.GeneratedBuilderReferenceInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.BuilderReferenceSpec.IntValue;
        }

        internal Phx.Inject.Tests.Data.Model.IntLeaf GetReferenceGetIntLeaf(
                Phx.Inject.Tests.GeneratedBuilderReferenceInjector.SpecContainerCollection specContainers
        ) {
            return intLeaf ??= Phx.Inject.Tests.BuilderReferenceSpec.GetIntLeaf(
                specContainers.GeneratedBuilderReferenceInjector_BuilderReferenceSpec.GetPropertyIntValue(specContainers));
        }

        internal void GetReferenceBuildLazyType(
                Phx.Inject.Tests.Data.Model.LazyType target,
                Phx.Inject.Tests.GeneratedBuilderReferenceInjector.SpecContainerCollection specContainers
        ) {
            Phx.Inject.Tests.BuilderReferenceSpec.BuildLazyType(
                target,
                specContainers.GeneratedBuilderReferenceInjector_BuilderReferenceSpec.GetReferenceGetIntLeaf(specContainers));
        }

        internal void GetReferenceBuildLazyTypeField(
                Phx.Inject.Tests.Data.Model.LazyType target,
                Phx.Inject.Tests.GeneratedBuilderReferenceInjector.SpecContainerCollection specContainers
        ) {
            Phx.Inject.Tests.BuilderReferenceSpec.BuildLazyTypeField(
                target,
                specContainers.GeneratedBuilderReferenceInjector_BuilderReferenceSpec.GetReferenceGetIntLeaf(specContainers));
        }
    }
}
#nullable restore
