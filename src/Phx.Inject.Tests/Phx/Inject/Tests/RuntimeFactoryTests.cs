﻿// -----------------------------------------------------------------------------
// <copyright file="RuntimeFactoryTests.cs" company="Star Cruise Studios LLC">
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

    public class RuntimeFactoryTests : LoggingTestClass {
        [Test]
        public void RuntimeFactoryIsInjected() {
            IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

            var leafFactory = When("Getting a type injected with a runtime factory", () => injector.GetLeafFactory());

            var leaf = Then("The runtime factory can be used to create an instance",
                () => {
                    var leaf = leafFactory.CreateLeaf();
                    Verify.That(leaf.IsNotNull());
                    return leaf;
                });
            Then("The correct value was returned",
                () => {
                    Verify.That(leaf.IsType<IntLeaf>());
                    Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(10));
                });
        }
        
        [Test]
        public void LabeledRuntimeFactoryIsInjected() {
            IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

            var leafFactory = When("Getting a type injected with a runtime factory", () => injector.GetLabeledLeafFactory());

            var leaf = Then("The runtime factory can be used to create an instance",
                () => {
                    var leaf = leafFactory.CreateLeaf();
                    Verify.That(leaf.IsNotNull());
                    return leaf;
                });
            Then("The correct value was returned",
                () => {
                    Verify.That(leaf.IsType<IntLeaf>());
                    Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(42));
                });
        }
        
        [Test]
        public void RuntimeFactoryIsProvidedOnInjector() {
            IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

            var leafFactory = When("Getting a type injected with a runtime factory", () => injector.GetLeafRuntimeFactory());

            var leaf = Then("The runtime factory can be used to create an instance",
                () => {
                    var leaf = leafFactory.Create();
                    Verify.That(leaf.IsNotNull());
                    return leaf;
                });
            Then("The correct value was returned",
                () => {
                    Verify.That(leaf.IsType<IntLeaf>());
                    Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(10));
                });
        }
        
        [Test]
        public void LabeledRuntimeFactoryIsProvidedOnInjector() {
            IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

            var leafFactory = When("Getting a type injected with a runtime factory", () => injector.GetLabeledLeafRuntimeFactory());

            var leaf = Then("The runtime factory can be used to create an instance",
                () => {
                    var leaf = leafFactory.Create();
                    Verify.That(leaf.IsNotNull());
                    return leaf;
                });
            Then("The correct value was returned",
                () => {
                    Verify.That(leaf.IsType<IntLeaf>());
                    Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(42));
                });
        }
    }
}
