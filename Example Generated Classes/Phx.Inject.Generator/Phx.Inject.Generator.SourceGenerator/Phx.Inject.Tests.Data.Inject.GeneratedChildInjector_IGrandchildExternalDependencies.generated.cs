﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests.Data.Inject {
    internal class GeneratedChildInjector_IGrandchildExternalDependencies : Phx.Inject.Tests.Data.External.IGrandchildExternalDependencies {
        private readonly Phx.Inject.Tests.Data.Inject.GeneratedChildInjector.SpecContainerCollection specContainers;

        public GeneratedChildInjector_IGrandchildExternalDependencies(Phx.Inject.Tests.Data.Inject.GeneratedChildInjector.SpecContainerCollection specContainers) {
            this.specContainers = specContainers;
        }

        public Phx.Inject.Tests.Data.Model.Node GetNode() {
            return specContainers.GeneratedChildInjector_ChildSpecification.GetNode(specContainers);
        }
    }
}
#nullable restore