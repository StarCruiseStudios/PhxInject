// -----------------------------------------------------------------------------
//  <copyright file="FactoryMethodTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data.Inject;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Test;
    using Phx.Validation;

    public class FactoryMethodTests : LoggingTestClass {
        [Test]
        public void AScopedFactoryIsInvoked() {
            IRawInjector injector = Given("A test injector.", () => new GeneratedRawInjector());

            var (root1, root2) = When("A factory method with scoped fabrication mode is invoked twice.",
                    () => (injector.GetRoot(), injector.GetRoot()));

            Then("The same instance is returned both times.", () => Verify.That(ReferenceEquals(root1, root2).IsTrue()));
        }

        [Test]
        public void ARecurrentFactoryIsInvoked() {
            IRawInjector injector = Given("A test injector.", () => new GeneratedRawInjector());

            var (node1, node2) = When("A factory method with recurrent fabrication mode is invoked twice.",
                    () => (injector.GetNode(), injector.GetNode()));

            Then("Different instances are returned each time.", () => Verify.That(ReferenceEquals(node1, node2).IsFalse()));
        }

        [Test]
        public void ALinkedFactoryIsInvoked() {
            IRawInjector injector = Given("A test injector.", () => new GeneratedRawInjector());

            var leaf = When("A factory method is constructed via a linked factory.", () => injector.GetILeaf());

            Then("The instance was constructed using the linked factory.", () => Verify.That(leaf.IsType<StringLeaf>()));
        }
    }
}
