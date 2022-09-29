// -----------------------------------------------------------------------------
//  <copyright file="InjectorTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data.Inject;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Test;
    using Phx.Validation;

    public class InjectorTests : LoggingTestClass {
        [Test]
        public void AnInjectorMethodIsGenerated() {
            IRawInjector injector = Given("A test injector.", () => new GeneratedRawInjector());

            var root = When("An injector method is invoked on the injector.", () => injector.GetRoot());

            Then("A valid value is constructed.", () => Verify.That(root.IsNotNull()));
        }

        [Test]
        public void AnInjectorBuilderMethodIsGenerated() {
            IRawInjector injector = Given("A test injector.", () => new GeneratedRawInjector());
            var lazyType = Given("An uninitialized lazy type.", () => new LazyType());

            When("An injector builder method is invoked on the injector.", () => injector.Build(lazyType));

            Then("The lazy type is initialized.", () => Verify.That(lazyType.Value.IsNotNull()));
        }

        [Test]
        public void InjectorsHaveDifferentScopes() {
            IRawInjector injector = Given("A test injector.", () => new GeneratedRawInjector());
            IRawInjector injector2 = Given("A second test injector.", () => new GeneratedRawInjector());

            var (root, root2) = When(
                    "The same scoped injector method is invoked on each injector.",
                    () => (injector.GetRoot(), injector2.GetRoot()));

            Then(
                    "Different instances are returned.",
                    () => Verify.That(
                            ReferenceEquals(root, root2)
                                    .IsFalse()));
        }
    }
}
