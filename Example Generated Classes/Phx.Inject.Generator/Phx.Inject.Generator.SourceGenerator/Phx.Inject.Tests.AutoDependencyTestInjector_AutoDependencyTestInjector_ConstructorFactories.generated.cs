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
    internal class AutoDependencyTestInjector_AutoDependencyTestInjector_ConstructorFactories {
        private Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode? autoTypeWithFabricationMode;

        internal AutoDependencyTestInjector_AutoDependencyTestInjector_ConstructorFactories CreateNewFrame() {
            return this;
        }

        internal Phx.Inject.Tests.Data.Model.AutoType GetConstructorautoType(
                Phx.Inject.Tests.AutoDependencyTestInjector.SpecContainerCollection specContainers
        ) {
            return new Phx.Inject.Tests.Data.Model.AutoType(
                specContainers.AutoDependencyTestInjector_CommonTestValueSpecification.GetInt(specContainers),
                specContainers.AutoDependencyTestInjector_AutoDependencyTestInjector_ConstructorFactories.GetConstructorautoTypeWithFabricationMode(specContainers));
        }

        internal Phx.Inject.Tests.Data.Model.IntLeaf GetConstructorintLeaf(
                Phx.Inject.Tests.AutoDependencyTestInjector.SpecContainerCollection specContainers
        ) {
            return new Phx.Inject.Tests.Data.Model.IntLeaf(
                specContainers.AutoDependencyTestInjector_CommonTestValueSpecification.GetInt(specContainers));
        }

        internal Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode GetConstructorautoTypeWithFabricationMode(
                Phx.Inject.Tests.AutoDependencyTestInjector.SpecContainerCollection specContainers
        ) {
            return autoTypeWithFabricationMode ??= new Phx.Inject.Tests.Data.Model.AutoTypeWithFabricationMode(
                specContainers.AutoDependencyTestInjector_CommonTestValueSpecification.GetInt(specContainers));
        }
    }
}
#nullable restore
