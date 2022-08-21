// -----------------------------------------------------------------------------
//  <copyright file="InjectorTests.cs" company="Star Cruise Studios LLC">
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

    public class FactoryReferenceTests : LoggingTestClass {
        [Test]
        public void AnInjectorMethodIsGenerated() {
            // IFactoryReferenceInjector injector = Given("A test injector.", () => new GeneratedFactoryReferenceInjector());
            //
            // var root = When("An injector method using a factory reference is.", () => injector.GetLeaf());
            //
            // Then("A valid value is constructed.", () => Verify.That(root.IsNotNull()));
        }

        [Test]
        public void AnInjectorBuilderMethodIsGenerated() {
            // IFactoryReferenceInjector injector = Given("A test injector.", () => new GeneratedFactoryReferenceInjector());
            // var lazyType = Given("An uninitialized lazy type.", () => new LazyType());
            //
            // When("An injector builder method using a builder reference is invoked.", () => injector.Build(lazyType));
            //
            // Then("The lazy type is initialized.", () => Verify.That(lazyType.Value.IsNotNull()));
        }
    }
}
