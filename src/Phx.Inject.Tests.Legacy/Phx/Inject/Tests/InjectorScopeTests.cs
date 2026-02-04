// -----------------------------------------------------------------------------
// <copyright file="InjectorScopeTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

using static InjectorScopeSpecification;

[Specification]
public static class InjectorScopeSpecification {
    public const string ScopedLabel = "Scoped";
    public const string RecurrentLabel = "Recurrent";
    public static int CurrentIntValue;

    [Factory]
    internal static int GetIntDefault() {
        return CurrentIntValue++;
    }

    // ReSharper disable once RedundantArgumentDefaultValue
    [Factory(FabricationMode.Recurrent)]
    [Label(RecurrentLabel)]
    internal static int GetIntRecurrent() {
        return CurrentIntValue++;
    }

    [Factory(FabricationMode.Scoped)]
    [Label(ScopedLabel)]
    internal static int GetIntScoped() {
        return CurrentIntValue++;
    }
}

[Injector(
    "InjectorScopeTestInjector",
    typeof(InjectorScopeSpecification))]
public interface IInjectorScopeTestInjector {
    int GetIntDefault();

    [Label(RecurrentLabel)]
    int GetIntRecurrent();

    [Label(ScopedLabel)]
    int GetIntScoped();
}

public class InjectorScopeTests : LoggingTestClass {
    [Test]
    public void DefaultScopeRecreatesDependencies() {
        IInjectorScopeTestInjector injector = Given("A test injector", () => new InjectorScopeTestInjector());

        var value = When("Getting a value", () => injector.GetIntDefault());
        var value2 = When("Getting a second value", () => injector.GetIntDefault());

        Then("The values are different", () => Verify.That(value.IsNotEqualTo(value2)));
    }

    [Test]
    public void RecurrentScopeRecreatesDependencies() {
        IInjectorScopeTestInjector injector = Given("A test injector", () => new InjectorScopeTestInjector());

        var value = When("Getting a value", () => injector.GetIntRecurrent());
        var value2 = When("Getting a second value", () => injector.GetIntRecurrent());

        Then("The values are different", () => Verify.That(value.IsNotEqualTo(value2)));
    }

    [Test]
    public void ScopedScopeReusesDependencies() {
        IInjectorScopeTestInjector injector = Given("A test injector", () => new InjectorScopeTestInjector());

        var value1 = When("Getting a value", () => injector.GetIntScoped());
        var value2 = When("Getting a second value", () => injector.GetIntScoped());

        Then("The values are the same", () => Verify.That(value1.IsEqualTo(value2)));
    }

    [Test]
    public void ScopedScopeIsNotReusedAcrossInjectors() {
        IInjectorScopeTestInjector injector1 = Given("A test injector", () => new InjectorScopeTestInjector());
        IInjectorScopeTestInjector injector2 = Given("Another test injector", () => new InjectorScopeTestInjector());

        var value1 = When("Getting a value from the first injector", () => injector1.GetIntScoped());
        var value2 = When("Getting a value from the secondInjector", () => injector2.GetIntScoped());

        Then("The values are different", () => Verify.That(value1.IsNotEqualTo(value2)));
    }
}
