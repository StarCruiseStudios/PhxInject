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
    internal partial class CustomNamedInjector : Phx.Inject.Tests.ICustomNamedInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.CustomNamedInjector_CommonTestValueSpecification CustomNamedInjector_CommonTestValueSpecification,
                Phx.Inject.Tests.CustomNamedInjector_CustomNamedInjector_ConstructorFactories CustomNamedInjector_CustomNamedInjector_ConstructorFactories
        ) {
            internal SpecContainerCollection CreateNewFrame() {
                return new SpecContainerCollection(
                        CustomNamedInjector_CommonTestValueSpecification.CreateNewFrame(),
                        CustomNamedInjector_CustomNamedInjector_ConstructorFactories.CreateNewFrame());
            }
        }

        private readonly SpecContainerCollection specContainers;

        public CustomNamedInjector() {
            specContainers = new SpecContainerCollection(
                    CustomNamedInjector_CommonTestValueSpecification: new Phx.Inject.Tests.Data.CustomNamedInjector_CommonTestValueSpecification(),
                    CustomNamedInjector_CustomNamedInjector_ConstructorFactories: new Phx.Inject.Tests.CustomNamedInjector_CustomNamedInjector_ConstructorFactories());
        }

        public System.Int32 GetInt() {
            return specContainers.CustomNamedInjector_CommonTestValueSpecification.GetInt(specContainers);
        }
    }
}
#nullable restore
