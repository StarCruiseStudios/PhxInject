﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.External {
    internal class GeneratedGrandchildInjector_IGrandchildExternalDependencies {
        private readonly Phx.Inject.Tests.Data.External.IGrandchildExternalDependencies instance;

        public GeneratedGrandchildInjector_IGrandchildExternalDependencies(Phx.Inject.Tests.Data.External.IGrandchildExternalDependencies instance) {
            this.instance = instance;
        }

        internal GeneratedGrandchildInjector_IGrandchildExternalDependencies CreateNewFrame() {
            return this;
        }

        internal Phx.Inject.Tests.Data.Model.Node GetNode(
                Phx.Inject.Tests.Data.Inject.GeneratedGrandchildInjector.SpecContainerCollection specContainers
        ) {
            return instance.GetNode();
        }
    }
}
#nullable restore
