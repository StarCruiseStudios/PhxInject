// -----------------------------------------------------------------------------
// <copyright file="LinkTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Test;
    using Phx.Validation;
    using static Data.CommonTestValueSpecification;

    [Injector(
        generatedClassName: "LinkTestInjector",
        typeof(CommonTestValueSpecification))]
    public interface ILinkTestInjector {
        ILeaf GetLinkedType();
    }
    
    public class LinkTests : LoggingTestClass {
        [Test]
        public void LinksCanBeUsedToAccessInjectedValues() {
            ILinkTestInjector injector = Given("A test injector", () => new LinkTestInjector());

            var value = When("Getting a linked value", () => injector.GetLinkedType());

            Then("The expected type was injected", () => Verify.That(value.IsType<IntLeaf>()));
            Then("The expected value was injected", IntValue, (expected) => Verify.That((value as IntLeaf)!.Value.IsEqualTo(expected)));
        }
    }
}
