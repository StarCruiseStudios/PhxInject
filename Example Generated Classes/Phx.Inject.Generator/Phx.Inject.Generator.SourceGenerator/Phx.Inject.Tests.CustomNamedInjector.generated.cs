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
                Phx.Inject.Tests.Data.CustomNamedInjector_CommonTestValueSpecification CustomNamedInjector_CommonTestValueSpecification
        ) {
            internal SpecContainerCollection CreateNewFrame() {
                return new SpecContainerCollection(
                        CustomNamedInjector_CommonTestValueSpecification.CreateNewFrame());
            }
        }

        private readonly SpecContainerCollection specContainers;

        public CustomNamedInjector() {
            specContainers = new SpecContainerCollection(
                    CustomNamedInjector_CommonTestValueSpecification: new Phx.Inject.Tests.Data.CustomNamedInjector_CommonTestValueSpecification());
        }

        public System.Int32 GetInt() {
            return specContainers.CustomNamedInjector_CommonTestValueSpecification.GetInt(specContainers);
        }
    }
}
#nullable restore
