// -----------------------------------------------------------------------------
// <copyright file="InjectorNamingTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data;
    using Phx.Test;
    using Phx.Validation;

    [Injector(typeof(CommonTestValueSpecification))]
    public interface IDefaultNamedInjector {
        int GetInt();
    }
    
    [Injector(
        generatedClassName: "CustomNamedInjector",
        typeof(CommonTestValueSpecification))]
    public interface ICustomNamedInjector {
        int GetInt();
    }

    public class InjectorNamingTests : LoggingTestClass {
        [Test]
        public void DefaultInjectorNameIsUsed() {
            var injector = When("Creating a default named injector", () => new GeneratedDefaultNamedInjector());
            Then("The injector was created", () => Verify.That(injector.IsNotNull()));
        }
        
        [Test]
        public void CustomInjectorNameIsUsed() {
            var injector = When("Creating a custom named injector", () => new CustomNamedInjector());
            Then("The injector was created", () => Verify.That(injector.IsNotNull()));
        }
    }
}
