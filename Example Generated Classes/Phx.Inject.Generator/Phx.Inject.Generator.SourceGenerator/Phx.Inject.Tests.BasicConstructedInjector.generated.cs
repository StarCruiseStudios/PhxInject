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
    internal partial class BasicConstructedInjector : Phx.Inject.Tests.IBasicConstructedInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.BasicConstructedInjector_IConstructedSpecification BasicConstructedInjector_IConstructedSpecification
        ) {
            internal SpecContainerCollection CreateNewFrame() {
                return new SpecContainerCollection(
                        BasicConstructedInjector_IConstructedSpecification.CreateNewFrame());
            }
        }

        private readonly SpecContainerCollection specContainers;

        public BasicConstructedInjector(
                Phx.Inject.Tests.IConstructedSpecification iConstructedSpecification
        ) {
            specContainers = new SpecContainerCollection(
                    BasicConstructedInjector_IConstructedSpecification: new Phx.Inject.Tests.BasicConstructedInjector_IConstructedSpecification(iConstructedSpecification));
        }

        public System.Int32 GetIntValue() {
            return specContainers.BasicConstructedInjector_IConstructedSpecification.GetIntValue(specContainers);
        }
    }
}
#nullable restore