// -----------------------------------------------------------------------------
// <copyright file="MultiBindTests.cs" company="Star Cruise Studios LLC">
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

    public class MultiBindTests : LoggingTestClass {
        [Test]
        public void MultiBindListIsInjected() {
            IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());
            
            var list = When("Getting a multibind list", () => injector.GetLeafList());
            
            Then("The list contains the expected values", () => {
                Verify.That(list.Count.IsEqualTo(2));
                Verify.That(list[0].IsType<IntLeaf>());
                Verify.That((list[0] as IntLeaf)!.Value.IsEqualTo(10));
                Verify.That(list[1].IsType<IntLeaf>());
                Verify.That((list[1] as IntLeaf)!.Value.IsEqualTo(20));
            });
        }
        
        [Test]
        public void MultiBindSetIsInjected() {
            IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());
            
            var set = When("Getting a multibind set", () => injector.GetLeafSet());
            
            Then("The list contains the expected values", () => {
                Verify.That(set.Count.IsEqualTo(2));
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 30).IsTrue());
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 40).IsTrue());
            });
        }
        
        [Test]
        public void MultiBindDictIsInjected() {
            IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());
            
            var dict = When("Getting a multibind list", () => injector.GetLeafDict());
            
            Then("The list contains the expected values", () => {
                Verify.That(dict.Count.IsEqualTo(2));
                Verify.That(dict["key1"].IsType<IntLeaf>());
                Verify.That((dict["key1"] as IntLeaf)!.Value.IsEqualTo(50));
                Verify.That(dict["key2"].IsType<IntLeaf>());
                Verify.That((dict["key2"] as IntLeaf)!.Value.IsEqualTo(60));
            });
        }
        [Test]
        public void MultiBindListFactoryIsInjected() {
            IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());
            
            var factory = When("Getting a multibind list", () => injector.GetLeafListRuntimeFactory());
            var list = factory.Create();
            
            Then("The list contains the expected values", () => {
                Verify.That(list.Count.IsEqualTo(2));
                Verify.That(list[0].IsType<IntLeaf>());
                Verify.That((list[0] as IntLeaf)!.Value.IsEqualTo(10));
                Verify.That(list[1].IsType<IntLeaf>());
                Verify.That((list[1] as IntLeaf)!.Value.IsEqualTo(20));
            });
        }
        
        [Test]
        public void MultiBindSetFactoryIsInjected() {
            IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());
            
            var factory = When("Getting a multibind set", () => injector.GetLeafSetRuntimeFactory());
            var set = factory.Create();
            
            Then("The list contains the expected values", () => {
                Verify.That(set.Count.IsEqualTo(2));
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 30).IsTrue());
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 40).IsTrue());
            });
        }
        
        [Test]
        public void MultiBindDictFactoryIsInjected() {
            IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());
            
            var factory = When("Getting a multibind list", () => injector.GetLeafDictRuntimeFactory());
            var dict = factory.Create();
            
            Then("The list contains the expected values", () => {
                Verify.That(dict.Count.IsEqualTo(2));
                Verify.That(dict["key1"].IsType<IntLeaf>());
                Verify.That((dict["key1"] as IntLeaf)!.Value.IsEqualTo(50));
                Verify.That(dict["key2"].IsType<IntLeaf>());
                Verify.That((dict["key2"] as IntLeaf)!.Value.IsEqualTo(60));
            });
        }
    }
}
