// -----------------------------------------------------------------------------
//  <copyright file="PropertyFactoryTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data.Inject;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;
    using Phx.Test;
    using Phx.Validation;

    public class PropertyFactoryTests : LoggingTestClass {
        [Test]
        public void APropertyCanBeUsedAsAFactoryMethodInAStaticSpec() {
            IPropertyFactoryInjector injector = Given(
                    "A test injector.",
                    () => new GeneratedPropertyFactoryInjector(new PropertyFactorySpec(new IntLeaf(10))));

            var result = When("A property factory method provided by a static spec is invoked on the injector.",
                    () => injector.GetStringLeaf());

            Then(
                    "The value from the constructed spec was returned.",
                    PropertyFactoryStaticSpec.StringValue,
                    (expected) => Verify.That(result.Value.IsEqualTo(expected)));
        }

        [Test]
        public void APropertyCanBeUsedAsAFactoryMethodInAConstructedSpec() {
            int expectedValue = Given("An int value", () => 10);
            IntLeaf inputLeaf = Given("An int leaf with the expected value.", () => new IntLeaf(expectedValue));
            IPropertyFactorySpec spec = Given(
                    "A constructed specification.",
                    () => new PropertyFactorySpec(inputLeaf));
            IPropertyFactoryInjector injector = Given(
                    "A test injector built with the spec.",
                    () => new GeneratedPropertyFactoryInjector(spec));

            var result = When("The property factory method is invoked on the injector.", () => injector.GetLeaf());

            Then(
                    "The value from the constructed spec was returned.",
                    expectedValue,
                    (expected) => Verify.That((result as IntLeaf)!.Value.IsEqualTo(expected)));
        }
    }
}
