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
    internal partial class InjectorBuilderTestInjector : Phx.Inject.Tests.IInjectorBuilderTestInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.Data.InjectorBuilderTestInjector_CommonTestValueSpecification InjectorBuilderTestInjector_CommonTestValueSpecification,
                Phx.Inject.Tests.InjectorBuilderTestInjector_InjectorBuilderTestInjector_ConstructorFactories InjectorBuilderTestInjector_InjectorBuilderTestInjector_ConstructorFactories
        ) {
            internal SpecContainerCollection CreateNewFrame() {
                return new SpecContainerCollection(
                        InjectorBuilderTestInjector_CommonTestValueSpecification.CreateNewFrame(),
                        InjectorBuilderTestInjector_InjectorBuilderTestInjector_ConstructorFactories.CreateNewFrame());
            }
        }

        private readonly SpecContainerCollection specContainers;

        public InjectorBuilderTestInjector() {
            specContainers = new SpecContainerCollection(
                    InjectorBuilderTestInjector_CommonTestValueSpecification: new Phx.Inject.Tests.Data.InjectorBuilderTestInjector_CommonTestValueSpecification(),
                    InjectorBuilderTestInjector_InjectorBuilderTestInjector_ConstructorFactories: new Phx.Inject.Tests.InjectorBuilderTestInjector_InjectorBuilderTestInjector_ConstructorFactories());
        }

        public void BuildTestBuilder(Phx.Inject.Tests.Data.Model.TestBuilderObject target) {
            specContainers.InjectorBuilderTestInjector_CommonTestValueSpecification.BuildTestBuilderObject(target, specContainers);
        }

        public void BuildTestBuilderLabelA(Phx.Inject.Tests.Data.Model.TestBuilderObject target) {
            specContainers.InjectorBuilderTestInjector_CommonTestValueSpecification.BuildTestBuilderObjectLabelA(target, specContainers);
        }
    }
}
#nullable restore
