// -----------------------------------------------------------------------------
//  <copyright file="InjectorTests.cs" company="Star Cruise Studios LLC">
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

    public class InjectorTests : LoggingTestClass {
        [Test]
        public void AnInjectorMethodIsGenerated() {
            ITestInjector injector = Given("A CustomInjector.", () => new CustomInjector());

            var root = When("An injector method is invoked on the injector.", () => injector.GetRoot());

            Then("A valid value is constructed.", () => Verify.That(root.IsNotNull()));
        }

        [Test]
        public void AnInjectorBuilderMethodIsGenerated() {
            ITestInjector injector = Given("A CustomInjector.", () => new CustomInjector());
            LazyType lazyType = Given("An uninitialized lazy type.", () => new LazyType());

            When("An injector builder method is invoked on the injector.", () => injector.Build(lazyType));

            Then("The lazy type is initialized.", () => Verify.That(lazyType.Value.IsNotNull()));
        }

        [Test]
        public void InjectorsHaveDifferentScopes() {
            ITestInjector injector = Given("A CustomInjector.", () => new CustomInjector());
            ITestInjector injector2 = Given("A second CustomInjector.", () => new CustomInjector());

            var (root, root2) = When("The same scoped injector method is invoked on each injector.", 
                    () => (injector.GetRoot(), injector2.GetRoot()));

            Then("Different instances are returned.", () => Verify.That(ReferenceEquals(root, root2).IsFalse()));
        }
    }
}