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
    using static Data.CommonTestValueSpecification;

    [Injector(
        generatedClassName: "InjectorBuilderTestInjector",
        typeof(CommonTestValueSpecification))]
    public interface IInjectorBuilderTestInjector {
        public void BuildTestBuilder(TestBuilderObject target);
        
        [Label(LabelA)]
        public void BuildTestBuilderLabelA(TestBuilderObject target);
    }
    
    public class InjectorBuilderTests : LoggingTestClass {
        [Test]
        public void BuildersCanBeUsedToLazilyInjectDependencies() {
            IInjectorBuilderTestInjector injector = Given("A test injector", () => new InjectorBuilderTestInjector());
            var target = Given("An uninitialized object", () => new TestBuilderObject());
            
            When("Initializing the object", () => injector.BuildTestBuilder(target));
            
            Then("The expected value was injected", IntValue, (expected) => Verify.That(target.IntValue.IsEqualTo(expected)));
        }
        
        [Test]
        public void LabeledAndUnlabeledValuesAreDifferent() {
            IInjectorBuilderTestInjector injector = Given("A test injector", () => new InjectorBuilderTestInjector());
            var unlabeled = Given("An uninitialized object", () => new TestBuilderObject());
            var labeled = Given("Another uninitialized object", () => new TestBuilderObject());

            When("Initializing the unlabeled object", () => injector.BuildTestBuilder(unlabeled));
            When("Initializing the labeled object", () => injector.BuildTestBuilderLabelA(labeled));

            Then("The values are different", () => Verify.That(unlabeled.IntValue.IsNotEqualTo(labeled.IntValue)));
        }
    }
}
