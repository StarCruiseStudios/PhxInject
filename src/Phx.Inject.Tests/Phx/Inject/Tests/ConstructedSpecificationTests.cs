// -----------------------------------------------------------------------------
//  <copyright file="ConstructedSpecificationTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data.Inject;
    using Phx.Inject.Tests.Data.Specification;
    using Phx.Test;
    using Phx.Validation;

    public class ConstructedSpecificationTests : LoggingTestClass {
        [Test]
        public void AConstructedSpecificationInjectorIsConstructed() {
            IConstructedSpecification spec = Given(
                    "A constructed specification.",
                    () => new ConstructedSpecificationImplementation());
            IConstructedInjector injector = Given(
                    "A test injector built with the spec.",
                    () => new GeneratedConstructedInjector(spec));

            var leaf = When("A factory method is invoked on the injector.", () => injector.GetIntLeaf());

            Then(
                    "The value from the constructed spec was returned.",
                    () => Verify.That(leaf.Value.IsEqualTo(ConstructedSpecificationImplementation.IntValue)));
        }
    }
}
