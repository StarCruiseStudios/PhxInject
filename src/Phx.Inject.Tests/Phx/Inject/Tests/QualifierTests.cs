// -----------------------------------------------------------------------------
//  <copyright file="QualifierTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data;
    using Phx.Test;
    using Phx.Validation;

    public class QualifierTests : LoggingTestClass {
        [Test]
        public void AQualifiedFactoryIsDifferentThanANonQualifiedFactory() {
            ILabelInjector injector = Given("A test injector.", () => new GeneratedLabelInjector());

            var (defaultLeaf, nonDefaultLeaf) = When("An unlabeled and labeled depenency are retrieved.", () => {
                return (injector.GetDefaultLeaf(), injector.GetNonDefaultLeafA());
            });

            Then("The correct default dependency was returned.", LabeledLeafSpecification.DefaultLeafData,
                (expected) => Verify.That((defaultLeaf as StringLeaf)!.Value.IsEqualTo(expected)));
            Then("The correct nondefault dependency was returned.", LabeledLeafSpecification.NonDefaultLeafAData,
                (expected) => Verify.That((nonDefaultLeaf as StringLeaf)!.Value.IsEqualTo(expected)));
        }

        [Test]
        public void AQualifiedFactoryIsDifferentThanAnotherQualifiedFactory() {
            ILabelInjector injector = Given("A test injector.", () => new GeneratedLabelInjector());

            var (leafA, leafB) = When("Two different labeled depenencies are retrieved.", () => {
                return (injector.GetNonDefaultLeafA(), injector.GetNonDefaultLeafB());
            });

            Then("The correct first dependency was returned.", LabeledLeafSpecification.NonDefaultLeafAData,
                (expected) => Verify.That((leafA as StringLeaf)!.Value.IsEqualTo(expected)));
            Then("The correct second dependency was returned.", LabeledLeafSpecification.NonDefaultLeafBData,
                (expected) => Verify.That((leafB as StringLeaf)!.Value.IsEqualTo(expected)));
        }

        [Test]
        public void AnAttributeQualifiedFactoryIsDifferentThanAnotherAttributeQualifiedFactory() {
            ILabelInjector injector = Given("A test injector.", () => new GeneratedLabelInjector());

            var (leafA, leafB) = When("Two different attribute qualified depenencies are retrieved.", () => {
                return (injector.GetAttributeNamedLeafA(), injector.GetAttributeNamedLeafB());
            });

            Then("The correct first dependency was returned.", LabeledLeafSpecification.AttributeNamedLeafAData,
                (expected) => Verify.That((leafA as StringLeaf)!.Value.IsEqualTo(expected)));
            Then("The correct second dependency was returned.", LabeledLeafSpecification.AttributeNamedLeafBData,
                (expected) => Verify.That((leafB as StringLeaf)!.Value.IsEqualTo(expected)));
        }

        [Test]
        public void ALabeledFactoryIsDifferentThanAnotherLabeledFactoryWithADifferentType() {
            ILabelInjector injector = Given("A test injector.", () => new GeneratedLabelInjector());

            var (leafA, leafB) = When("Two different attribute qualified depenencies are retrieved.", () => {
                return (injector.GetStringNamedLeafA(), injector.GetNamedStringLeaf());
            });

            Then("The correct first dependency was returned.", LabeledLeafSpecification.StringNamedLeafAData,
                (expected) => Verify.That((leafA as StringLeaf)!.Value.IsEqualTo(expected)));
            Then("The correct second dependency was returned.", LabeledLeafSpecification.NamedStringLeafData,
                (expected) => Verify.That((leafB as StringLeaf)!.Value.IsEqualTo(expected)));
        }

        [Test]
        public void DifferentLabeledDependenciesAreInjected() {
            ILabelInjector injector = Given("A test injector.", () => new GeneratedLabelInjector());

            var node = When("A dependency is retrieved that was injected with two different qualified depenencies.", () => {
                return injector.GetNode();
            });

            Then("The correct left dependency was returned.", LabeledLeafSpecification.NonDefaultLeafAData,
                (expected) => Verify.That((node.Left as StringLeaf)!.Value.IsEqualTo(expected)));
            Then("The correct right dependency was returned.", LabeledLeafSpecification.AttributeNamedLeafAData,
                (expected) => Verify.That((node.Right as StringLeaf)!.Value.IsEqualTo(expected)));
        }


        [Test]
        public void AnUnlabeledBuilderReturnsDifferentDependenciesThanALabeledBuilder() {
            ILabelInjector injector = Given("A test injector.", () => new GeneratedLabelInjector());

            var unlabeledLazyType = When("An unlabeled dependency is built.", () => {
                var lazyType = new LazyType();
                injector.BuildUnlabeledLazyType(lazyType);
                return lazyType;
            });

            var labeledLazyType = When("A labeled dependency is built.", () => {
                var lazyType = new LazyType();
                injector.BuildLabeledLazyType(lazyType);
                return lazyType;
            });

            Then("The correct unlabeled dependency was returned.", LabeledLeafSpecification.DefaultLeafData,
                (expected) => Verify.That((unlabeledLazyType.Value as StringLeaf)!.Value.IsEqualTo(expected)));
            Then("The correct labeled dependency was returned.", LabeledLeafSpecification.NonDefaultLeafAData,
                (expected) => Verify.That((labeledLazyType.Value as StringLeaf)!.Value.IsEqualTo(expected)));
        }
    }
}