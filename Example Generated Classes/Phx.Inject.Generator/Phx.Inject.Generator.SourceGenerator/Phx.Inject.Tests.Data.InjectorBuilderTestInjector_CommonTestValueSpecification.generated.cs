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
    internal class InjectorBuilderTestInjector_CommonTestValueSpecification {

        internal InjectorBuilderTestInjector_CommonTestValueSpecification CreateNewFrame() {
            return this;
        }

        internal System.Int32 GetInt(
                Phx.Inject.Tests.InjectorBuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetInt();
        }

        internal System.Int32 GetIntLabelA(
                Phx.Inject.Tests.InjectorBuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetIntLabelA();
        }

        internal System.String GetStringLabelA(
                Phx.Inject.Tests.InjectorBuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetStringLabelA();
        }

        internal System.Int32 GetIntQualifierA(
                Phx.Inject.Tests.InjectorBuilderTestInjector.SpecContainerCollection specContainers
        ) {
            return Phx.Inject.Tests.Data.CommonTestValueSpecification.GetIntQualifierA();
        }

        internal void BuildTestBuilderObject(
                Phx.Inject.Tests.Data.TestBuilderObject target,
                Phx.Inject.Tests.InjectorBuilderTestInjector.SpecContainerCollection specContainers
        ) {
            Phx.Inject.Tests.Data.CommonTestValueSpecification.BuildTestBuilderObject(
                target,
                specContainers.InjectorBuilderTestInjector_CommonTestValueSpecification.GetInt(specContainers));
        }

        internal void BuildTestBuilderObjectLabelA(
                Phx.Inject.Tests.Data.TestBuilderObject target,
                Phx.Inject.Tests.InjectorBuilderTestInjector.SpecContainerCollection specContainers
        ) {
            Phx.Inject.Tests.Data.CommonTestValueSpecification.BuildTestBuilderObjectLabelA(
                target,
                specContainers.InjectorBuilderTestInjector_CommonTestValueSpecification.GetIntLabelA(specContainers));
        }
    }
}
#nullable restore
