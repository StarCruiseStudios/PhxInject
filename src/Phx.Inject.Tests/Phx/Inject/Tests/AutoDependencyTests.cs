// -----------------------------------------------------------------------------
// <copyright file="AutoDependencyTests.cs" company="Star Cruise Studios LLC">
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
        generatedClassName: "AutoDependencyTestInjector",
        typeof(CommonTestValueSpecification))]
    public interface IAutoDependencyTestInjector {
        IntLeaf GetAutoType();
    }
    
    public class AutoDependencyTests : LoggingTestClass {
        [Test]
        public void LinksCanBeUsedToAccessInjectedValues() {
            IAutoDependencyTestInjector injector = Given("A test injector", () => new AutoDependencyTestInjector());

            var value = When("Getting a auto dependency value", () => injector.GetAutoType());

            Then("The expected value was injected", IntValue, (expected) => Verify.That(value.Value.IsEqualTo(expected)));
        }
    }
}
