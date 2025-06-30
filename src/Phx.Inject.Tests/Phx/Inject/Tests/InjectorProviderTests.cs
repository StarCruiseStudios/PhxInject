// -----------------------------------------------------------------------------
// <copyright file="InjectorNamingTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

using static CommonTestValueSpecification;

[Injector(
    "InjectorProviderTestInjector",
    typeof(CommonTestValueSpecification))]
public interface IInjectorProviderTestInjector {
    int GetInt();

    [Label(LabelA)]
    int GetLabelAInt();

    [Label(LabelA)]
    string GetlabelAString();

    [QualifierA]
    int GetIntQualifierA();

    TestGenericObject<int> GetGenericObject();
}

public class InjectorProviderTests : LoggingTestClass {
    [Test]
    public void ProvidersCanBeUsedToAccessInjectedValues() {
        IInjectorProviderTestInjector injector = Given("A test injector", () => new InjectorProviderTestInjector());

        var value = When("Getting a value", () => injector.GetInt());

        Then("The expected value was injected", IntValue, expected => Verify.That(value.IsEqualTo(expected)));
    }

    [Test]
    public void LabeledAndUnlabeledValuesAreDifferent() {
        IInjectorProviderTestInjector injector = Given("A test injector", () => new InjectorProviderTestInjector());

        var unlabeled = When("Getting an unlabeled value", () => injector.GetInt());
        var labeled = When("Getting a labeled value", () => injector.GetLabelAInt());

        Then("The values are different", () => Verify.That(unlabeled.IsNotEqualTo(labeled)));
    }

    [Test]
    public void ALabelCanBeReusedWithDifferentTypes() {
        IInjectorProviderTestInjector injector = Given("A test injector", () => new InjectorProviderTestInjector());

        var labeledInt = When("Getting a labeled int value", () => injector.GetLabelAInt());
        var labeledString = When("Getting a labeled string value", () => injector.GetlabelAString());

        Then("The expected value was injected",
            LabelAIntValue,
            expected => Verify.That(labeledInt.IsEqualTo(expected)));
        Then("The expected value was injected",
            LabelAStringValue,
            expected => Verify.That(labeledString.IsEqualTo(expected)));
    }

    [Test]
    public void QualifiedAndUnqualifiedValuesAreDifferent() {
        IInjectorProviderTestInjector injector = Given("A test injector", () => new InjectorProviderTestInjector());

        var unqualified = When("Getting an unqualified value", () => injector.GetInt());
        var qualified = When("Getting a qualified value", () => injector.GetIntQualifierA());

        Then("The values are different", () => Verify.That(unqualified.IsNotEqualTo(qualified)));
    }

    [Test]
    public void ProvidersCanBeUsedToAccessGenericInjectedValues() {
        IInjectorProviderTestInjector injector = Given("A test injector", () => new InjectorProviderTestInjector());

        TestGenericObject<int> value = When("Getting a generic value", () => injector.GetGenericObject());

        Then("The expected value was injected", IntValue, expected => Verify.That(value.Value.IsEqualTo(expected)));
    }
}
