﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.9.1.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests {
    internal class BuilderTestInjector_IBuilderTestInjector_ConstructorFactories {
        private Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode? autoTypeWithFabricationMode;

        internal BuilderTestInjector_IBuilderTestInjector_ConstructorFactories CreateNewFrame() {
            return this;
        }

        internal Phx.Inject.Tests.Data.Model.AutoType CtorFac_AutoType_AutoType(
                Phx.Inject.Tests.BuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return new Phx.Inject.Tests.Data.Model.AutoType(
                specContainers.BuilderTestInjector_CommonTestValueSpecification.Fac_Int32_GetInt(specContainers),
                specContainers.BuilderTestInjector_IBuilderTestInjector_ConstructorFactories.CtorFac_AutoTypeWithFabricationMode_AutoTypeWithFabricationMode(specContainers));
        }

        internal Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode CtorFac_AutoTypeWithFabricationMode_AutoTypeWithFabricationMode(
                Phx.Inject.Tests.BuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return autoTypeWithFabricationMode ??= new Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode(
                specContainers.BuilderTestInjector_CommonTestValueSpecification.Fac_Int32_GetInt(specContainers));
        }

        internal Phx.Inject.Tests.Data.Model.AutoTypeWithRequiredProperties CtorFac_AutoTypeWithRequiredProperties_AutoTypeWithRequiredProperties(
                Phx.Inject.Tests.BuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return new Phx.Inject.Tests.Data.Model.AutoTypeWithRequiredProperties(
                specContainers.BuilderTestInjector_IBuilderTestInjector_ConstructorFactories.CtorFac_AutoType_AutoType(specContainers)
            ) {
                X = specContainers.BuilderTestInjector_CommonTestValueSpecification.Fac_Int32_GetInt(specContainers),
                Y = specContainers.BuilderTestInjector_IBuilderTestInjector_ConstructorFactories.CtorFac_AutoTypeWithFabricationMode_AutoTypeWithFabricationMode(specContainers)
            };
        }

        internal Phx.Inject.Tests.Data.Model.IntLeaf CtorFac_IntLeaf_IntLeaf(
                Phx.Inject.Tests.BuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return new Phx.Inject.Tests.Data.Model.IntLeaf(
                specContainers.BuilderTestInjector_CommonTestValueSpecification.Fac_Int32_GetInt(specContainers));
        }
    }
}
#nullable restore
