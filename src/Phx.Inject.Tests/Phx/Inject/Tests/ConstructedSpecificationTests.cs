// -----------------------------------------------------------------------------
//  <copyright file="ConstructedSpecificationTests.cs" company="Star Cruise Studios LLC">
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

        [Test]
        public void AConstructedSpecificationCanHaveAutoFactories() {
            IConstructedSpecification spec = Given(
                    "A constructed specification.",
                    () => new ConstructedSpecificationImplementation());
            IConstructedInjector injector = Given(
                    "A test injector built with the spec.",
                    () => new GeneratedConstructedInjector(spec));

            var outer = When("A factory method is invoked on the injector using an auto dependency.",
                    () => injector.GetOuterType());

            Then(
                    "The auto injected type was returned.",
                    () => { Verify.That(outer.AutoType.IsType<AutoType>()); });
        }

        [Test]
        public void AConstructedSpecificationCanHaveLinkAttributes() {
            IConstructedSpecification spec = Given(
                    "A constructed specification.",
                    () => new ConstructedSpecificationImplementation());
            IConstructedInjector injector = Given(
                    "A test injector built with the spec.",
                    () => new GeneratedConstructedInjector(spec));

            ILeaf leaf = When("A factory method is invoked on the injector using a linked dependency.",
                    () => injector.GetILeaf());

            Then(
                    "The linked type was returned.",
                    () => { Verify.That(leaf.IsType<IntLeaf>()); });
        }

        [Test]
        public void AConstructedSpecificationInjectorCanBeAChild() {
            IConstructedSpecification spec = Given(
                    "A constructed specification.",
                    () => new ConstructedSpecificationImplementation());
            IConstructedParentInjector injector = Given(
                    "A test parent injector.",
                    () => new GeneratedConstructedParentInjector());

            var childInjector = When("A child injector is retrieved that uses the constructed specification.",
                    () => injector.GetChildInjector(spec));

            Then(
                    "The value from the constructed spec was returned.",
                    () => Verify.That(childInjector.GetIntLeaf().Value
                            .IsEqualTo(ConstructedSpecificationImplementation.IntValue)));
        }
    }
}
