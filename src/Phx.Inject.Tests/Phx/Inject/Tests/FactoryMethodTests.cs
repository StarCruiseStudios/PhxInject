// -----------------------------------------------------------------------------
//  <copyright file="FactoryMethodTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data;
    using Phx.Test;
    using Phx.Validation;

    public class FactoryMethodTests : LoggingTestClass {
        [Test]
        public void AScopedFactoryIsInvoked() {
            ITestInjector injector = Given("A CustomInjector.", () => new CustomInjector());

            var (root1, root2) = When("A factory method with scoped fabrication mode is invoked twice.", 
                    () => (injector.GetRoot(), injector.GetRoot()));

            Then("The same instance is returned both times.", () => Verify.That(ReferenceEquals(root1, root2).IsTrue()));
        }

        [Test]
        public void ARecurrentFactoryIsInvoked() {
            ITestInjector injector = Given("A CustomInjector.", () => new CustomInjector());
            var root = Given("A root instance created by the injector.", () => injector.GetRoot());

            var (node1, node2) = When("A factory method with recurrent fabrication mode is invoked twice while constructing the root.",
                    () => (root.Node, root.SecondaryNode));

            Then("Different instances are returned each time.", () => Verify.That(ReferenceEquals(node1, node2).IsFalse()));
        }

        [Test]
        public void ALinkedFactoryIsInvoked() {
            ITestInjector injector = Given("A CustomInjector.", () => new CustomInjector());
            var node = Given("A node instance created by the injector.", () => injector.GetRoot().Node);

            var leaf = When("The leaf of the node is constructed via a linked factory.", () => node.Right);

            Then("The leaf was constructed using the linked factory.", () => Verify.That(leaf.IsType<StringLeaf>()));
        }
    }
}
