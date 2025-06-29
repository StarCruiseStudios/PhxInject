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
        OuterType GetOuterType();
        
        void BuildAutoBuilderType(AutoBuilderType autoBuilderType);
        
        [Label(nameof(AutoBuilderType))] 
        void BuildLabeledAutoBuilderType(AutoBuilderType autoBuilderType);
    }
    
    public class AutoDependencyTests : LoggingTestClass {
        [Test]
        public void FactoriesCanBeAutomaticallyGenerated() {
            IAutoDependencyTestInjector injector = Given("A test injector", () => new AutoDependencyTestInjector());

            var outerType = When("Getting a auto dependency value", () => injector.GetOuterType());
            var value = outerType.AutoType;
            var value2 = outerType.AutoTypeWithRequiredProperties;

            Then("The expected value was injected", IntValue, (expected) => Verify.That(value.Value.IsEqualTo(expected)));
            Then("The expected value was injected", IntValue, (expected) => Verify.That(value2.X.IsEqualTo(expected)));
        }
        
        [Test]
        public void ScopedFactoriesCanBeAutomaticallyGenerated() {
            IAutoDependencyTestInjector injector = Given("A test injector", () => new AutoDependencyTestInjector());

            
            var outerType = When("Getting a auto dependency value", () => injector.GetOuterType());
            var value = outerType.AutoType.AutoTypeWithFabricationMode;
            var value2 = outerType.AutoTypeWithRequiredProperties;

            Then("The expected value was injected", 10, (expected) => Verify.That(value.X.IsEqualTo(expected)));
        }
        
        [Test]
        public void BuildersCanBeAutomaticallyGenerated() {
            IAutoDependencyTestInjector injector = Given("A test injector", () => new AutoDependencyTestInjector());
            AutoBuilderType autoBuilderType = When("Getting a auto builder type", () => new AutoBuilderType());
            AutoBuilderType labeledAutoBuilderType = When("Getting a labeled auto builder type", () => new AutoBuilderType());

            When("Getting a injecting the auto builder value", () => injector.BuildAutoBuilderType(autoBuilderType));
            Then("The expected value was injected", IntValue, (expected) => Verify.That(autoBuilderType.Value.IsEqualTo(expected)));
            
            When("Getting a injecting the labeled auto builder value", () => injector.BuildLabeledAutoBuilderType(labeledAutoBuilderType));
            Then("The expected value was injected", IntValue + IntValue, (expected) => Verify.That(labeledAutoBuilderType.Value.IsEqualTo(expected)));
        }
    }
}
