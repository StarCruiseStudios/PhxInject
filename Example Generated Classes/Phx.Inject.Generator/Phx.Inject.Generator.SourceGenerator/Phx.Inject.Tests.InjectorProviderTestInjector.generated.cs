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
    internal partial class InjectorProviderTestInjector : Phx.Inject.Tests.IInjectorProviderTestInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.InjectorProviderTestInjector_CommonTestValueSpecification InjectorProviderTestInjector_CommonTestValueSpecification,
                Phx.Inject.Tests.InjectorProviderTestInjector_InjectorProviderTestInjector_ConstructorFactories InjectorProviderTestInjector_InjectorProviderTestInjector_ConstructorFactories
        ) {
            internal SpecContainerCollection CreateNewFrame() {
                return new SpecContainerCollection(
                        InjectorProviderTestInjector_CommonTestValueSpecification.CreateNewFrame(),
                        InjectorProviderTestInjector_InjectorProviderTestInjector_ConstructorFactories.CreateNewFrame());
            }
        }

        private readonly SpecContainerCollection specContainers;

        public InjectorProviderTestInjector() {
            specContainers = new SpecContainerCollection(
                    InjectorProviderTestInjector_CommonTestValueSpecification: new Phx.Inject.Tests.Data.InjectorProviderTestInjector_CommonTestValueSpecification(),
                    InjectorProviderTestInjector_InjectorProviderTestInjector_ConstructorFactories: new Phx.Inject.Tests.InjectorProviderTestInjector_InjectorProviderTestInjector_ConstructorFactories());
        }

        public System.Int32 GetInt() {
            return specContainers.InjectorProviderTestInjector_CommonTestValueSpecification.GetInt(specContainers);
        }

        public System.Int32 GetLabelAInt() {
            return specContainers.InjectorProviderTestInjector_CommonTestValueSpecification.GetIntLabelA(specContainers);
        }

        public System.String GetlabelAString() {
            return specContainers.InjectorProviderTestInjector_CommonTestValueSpecification.GetStringLabelA(specContainers);
        }

        public System.Int32 GetIntQualifierA() {
            return specContainers.InjectorProviderTestInjector_CommonTestValueSpecification.GetIntQualifierA(specContainers);
        }
    }
}
#nullable restore
