// -----------------------------------------------------------------------------
// <copyright file="InjectorNamingTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

[Injector(typeof(CommonTestValueSpecification))]
public interface IDefaultNamedInjector {
    int GetInt();
}

[Injector(
    typeof(CommonTestValueSpecification),
    GeneratedClassName = "CustomNamedInjector"
)]
public interface ICustomNamedInjector {
    int GetInt();
}

public class InjectorNamingTests : LoggingTestClass {
    [Test]
    public void DefaultInjectorNameIsUsed() {
        // var injector = When("Creating a default named injector", () => new GeneratedDefaultNamedInjector());
        // Then("The injector was created", () => Verify.That(injector.IsNotNull()));
    }

    [Test]
    public void CustomInjectorNameIsUsed() {
        // var injector = When("Creating a custom named injector", () => new CustomNamedInjector());
        // Then("The injector was created", () => Verify.That(injector.IsNotNull()));
    }
}
