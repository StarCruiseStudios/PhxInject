// -----------------------------------------------------------------------------
// <copyright file="BuilderTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
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
    "BuilderTestInjector",
    typeof(CommonTestValueSpecification))]
public interface IBuilderTestInjector {
    public void BuildTestBuilder(TestBuilderObject target);

    [Label(LabelA)]
    public void BuildTestBuilderLabelA(TestBuilderObject target);
}

public class BuilderTests : LoggingTestClass {
    [Test]
    public void BuildersCanBeUsedToLazilyInjectDependencies() {
        IBuilderTestInjector builderTestInjector = Given("A test injector", () => new BuilderTestInjector());
        var target = Given("An uninitialized object", () => new TestBuilderObject());

        When("Initializing the object", () => builderTestInjector.BuildTestBuilder(target));

        Then("The expected value was injected", IntValue, expected => Verify.That(target.IntValue.IsEqualTo(expected)));
    }

    [Test]
    public void LabeledAndUnlabeledValuesAreDifferent() {
        IBuilderTestInjector builderTestInjector = Given("A test injector", () => new BuilderTestInjector());
        var unlabeled = Given("An uninitialized object", () => new TestBuilderObject());
        var labeled = Given("Another uninitialized object", () => new TestBuilderObject());

        When("Initializing the unlabeled object", () => builderTestInjector.BuildTestBuilder(unlabeled));
        When("Initializing the labeled object", () => builderTestInjector.BuildTestBuilderLabelA(labeled));

        Then("The values are different", () => Verify.That(unlabeled.IntValue.IsNotEqualTo(labeled.IntValue)));
    }
}
