﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data {
    internal class CustomNamedInjector_CommonTestValueSpecification {

        internal CustomNamedInjector_CommonTestValueSpecification CreateNewFrame() {
            return this;
        }

        internal System.Int32 GetInt(
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetInt();
        }

        internal System.Int32 GetIntLabelA(
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetIntLabelA();
        }

        internal System.String GetStringLabelA(
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetStringLabelA();
        }

        internal System.Int32 GetIntQualifierA(
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetIntQualifierA();
        }

        internal Phx.Inject.Tests.Data.Model.TestGenericObject<System.Int32> GetGenericObject(
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetGenericObject(
                specContainers.CustomNamedInjector_CommonTestValueSpecification.GetInt(specContainers));
        }

        internal Phx.Inject.Tests.Data.Model.OuterType GetOuterType(
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetOuterType(
                specContainers.CustomNamedInjector_CustomNamedInjector_ConstructorFactories.GetConstructorautoType(specContainers));
        }

        internal void BuildTestBuilderObject(
                Phx.Inject.Tests.Data.Model.TestBuilderObject target,
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            Phx.Inject.Tests.Data.CommonTestValueSpecification.BuildTestBuilderObject(
                target,
                specContainers.CustomNamedInjector_CommonTestValueSpecification.GetInt(specContainers));
        }

        internal void BuildTestBuilderObjectLabelA(
                Phx.Inject.Tests.Data.Model.TestBuilderObject target,
                Phx.Inject.Tests.CustomNamedInjector.SpecContainerCollection specContainers
        ) {
            Phx.Inject.Tests.Data.CommonTestValueSpecification.BuildTestBuilderObjectLabelA(
                target,
                specContainers.CustomNamedInjector_CommonTestValueSpecification.GetIntLabelA(specContainers));
        }
    }
}
#nullable restore
