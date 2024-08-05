// -----------------------------------------------------------------------------
// <copyright file="ContainerTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data.Inject;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Test;
    using Phx.Validation;

    public class ContainerTests : LoggingTestClass {
        [Test]
        public void ContainersReuseTypes() {
            IContainerInjector injector = Given("A test injector.", () => new GeneratedContainerInjector());

            var node = When("Getting a container type", () => injector.GetNode());

            Then("The container reuses the same instance of a type",
                () => {
                    Verify.That(node.Left.IsReferenceNotEqualTo(node.Right));
                    Verify.That((node.Left as IntLeaf)!.Value.IsEqualTo((node.Right as IntLeaf)!.Value));
                });
        }
        
        [Test]
        public void DifferentContainersDontReuseTypes() {
            IContainerInjector injector = Given("A test injector.", () => new GeneratedContainerInjector());

            var node1 = When("Getting a container type", () => injector.GetNode());
            var node2 = When("Getting another container type", () => injector.GetNode());

            Then("The first container reuses the same instance of a type",
                () => {
                    Verify.That(node1.Left.IsReferenceNotEqualTo(node1.Right));
                    Verify.That((node1.Left as IntLeaf)!.Value.IsEqualTo((node1.Right as IntLeaf)!.Value));
                });
            
            Then("The second container reuses the same instance of a type",
                () => {
                    Verify.That(node2.Left.IsReferenceNotEqualTo(node2.Right));
                    Verify.That((node2.Left as IntLeaf)!.Value.IsEqualTo((node2.Right as IntLeaf)!.Value));
                });
            
            Then("The container have different instances",
                () => {
                    Verify.That((node1.Left as IntLeaf)!.Value.IsNotEqualTo((node2.Left as IntLeaf)!.Value));
                    Verify.That((node1.Right as IntLeaf)!.Value.IsNotEqualTo((node2.Right as IntLeaf)!.Value));
                });
        }
    }
}
