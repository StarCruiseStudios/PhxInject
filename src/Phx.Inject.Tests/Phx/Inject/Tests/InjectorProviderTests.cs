﻿// -----------------------------------------------------------------------------
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
        generatedClassName: "InjectorProviderTestInjector",
        typeof(CommonTestValueSpecification))]
    public interface IInjectorProviderTestInjector {
        int GetInt();
        
        [Label(LabelA)]
        int GetLabelAInt();
        
        [Label(LabelA)]
        string GetlabelAString();
    }
    
    public class InjectorProviderTests : LoggingTestClass {
        [Test]
        public void ProvidersCanBeUsedToAccessInjectedValues() {
            IInjectorProviderTestInjector injector = Given("A test injector", () => new InjectorProviderTestInjector());

            var value = When("Getting a value", () => injector.GetInt());
            
            Then("The expected value was injected", IntValue, (expected) => Verify.That(value.IsEqualTo(expected)));
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
            
            Then("The expected value was injected", LabelAIntValue, (expected) => Verify.That(labeledInt.IsEqualTo(expected)));
            Then("The expected value was injected", LabelAStringValue, (expected) => Verify.That(labeledString.IsEqualTo(expected)));
        }
    }
}
